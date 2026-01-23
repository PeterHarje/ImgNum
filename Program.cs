using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

static class ImgNum
{
    // ImgNum - Image <-> Big Integer (base-256) encoder/decoder
    //
    // Programmer: Algot Rydberg "AggE"
    // Idea:       Peter Härje "Endarker"
    //
    // Pixel order: Top-left -> bottom-right (row-major)
    // Color model: RGB 8-bit (alpha discarded)
    // Storage:     Interprets the full RGB byte stream as one big-endian unsigned integer.
    //              Saved to .imgnum with a small header (width/height/channels/order/lengths).
    //
    // Universe note (Frankenstein Junkpile):
    // This tool is a fragment sent back in time to help future beings preserve identity.
    // When humanity evolves into energy-forms and hybrid bodies (flesh machines + metal machines),
    // memory becomes currency. ImgNum is one method to anchor a "life-frame" into a single number.

    private static readonly byte[] MAGIC = System.Text.Encoding.ASCII.GetBytes("IMGNUM"); // 6 bytes
    private const byte VERSION = 1;

    // ORDER = 1 means top-left -> bottom-right row-major (y=0..H-1, x=0..W-1)
    private const byte ORDER_ROWMAJOR_TL_BR = 1;

    // Seal text format
    private const string SEAL_HEADER = "IMGNUM_SEAL_V1";
    private const string SEAL_TAG = "FRANKENSTEIN_JUNKPILE_FRAGMENT";
    private const int SHORT_ID_HEX_LEN = 16; // 16 hex chars = 64-bit fingerprint

    private sealed class Header
    {
        public uint Width;
        public uint Height;
        public byte Channels;
        public byte Order;
        public int TotalBytes;   // int for array sizes
        public byte[] Payload = Array.Empty<byte>();
    }

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length < 1) return Usage("Missing command.");

            string cmd = args[0].ToLowerInvariant();
            return cmd switch
            {
                "encode" => EncodeCmd(args),
                "decode" => DecodeCmd(args),
                "decimal" => DecimalCmd(args),
                "info" => InfoCmd(args),
                "hash" => HashCmd(args),
                "verify" => VerifyCmd(args),
                "seal" => SealCmd(args),
                "checkseal" => CheckSealCmd(args),
                "batchseal" => BatchSealCmd(args),
                _ => Usage($"Unknown command: {args[0]}")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ERROR: " + ex.Message);
            return 2;
        }
    }

    private static int Usage(string? error = null)
    {
        if (!string.IsNullOrWhiteSpace(error))
            Console.Error.WriteLine("ERROR: " + error);

        Console.WriteLine(@"
IMGNUM // FRANKENSTEIN JUNKPILE TRANSMISSION
Artifact Class: Memory Anchor
Protocol: Image → Integer → Image

Commands:
  encode <inputImage> <output.imgnum>
  decode <input.imgnum> <outputImage.png>
  info  <input.imgnum>
  hash  <input.imgnum>
  verify <input.imgnum> <imageToCompare>
  seal <input.imgnum> <output.seal>
  checkseal <input.imgnum> <input.seal>
  batchseal <folder> [--recursive] [--index <path>]
  decimal <input.imgnum> <outputNumber.txt>   (WARNING: can be gigantic)

Notes:
- Pixel order: Top-left -> bottom-right (row-major).
- Colors: stored as RGB 8-bit (3 bytes per pixel). Alpha is discarded.
- .imgnum stores a minimal integer payload + TotalBytes to restore leading zeros.
");
        return 1;
    }

    // ---------- Commands ----------

    private static int EncodeCmd(string[] args)
    {
        if (args.Length < 3) return Usage("encode requires <inputImage> <output.imgnum>.");

        string input = args[1];
        string output = args[2];

        using Image<Rgb24> img = Image.Load<Rgb24>(input); // converts to RGB8
        uint w = (uint)img.Width;
        uint h = (uint)img.Height;
        byte channels = 3;

        int totalBytes = checked(img.Width * img.Height * (int)channels);
        if (totalBytes <= 0) throw new InvalidOperationException("Image has zero size.");

        byte[] data = new byte[totalBytes];
        int idx = 0;

        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                Rgb24 p = img[x, y];
                data[idx++] = p.R;
                data[idx++] = p.G;
                data[idx++] = p.B;
            }
        }

        BigInteger N = new BigInteger(data, isUnsigned: true, isBigEndian: true);

        byte[] payload = N.ToByteArray(isUnsigned: true, isBigEndian: true);
        if (payload.Length == 0) payload = new byte[] { 0x00 };

        WriteImgNum(
            output,
            w, h,
            channels,
            ORDER_ROWMAJOR_TL_BR,
            (ulong)totalBytes,
            (ulong)payload.Length,
            payload
        );

        Console.WriteLine($"Encoded: {input}");
        Console.WriteLine($"Size:    {w}x{h}");
        Console.WriteLine($"Bytes:   {totalBytes}");
        Console.WriteLine($"Saved:   {output} (payload bytes={payload.Length})");
        return 0;
    }

    private static int DecodeCmd(string[] args)
    {
        if (args.Length < 3) return Usage("decode requires <input.imgnum> <outputImage>.");

        string input = args[1];
        string output = args[2];

        Header hdr = ReadImgNum(input);

        if (hdr.Channels != 3) throw new NotSupportedException("Only RGB (3 channels) is supported.");
        if (hdr.Order != ORDER_ROWMAJOR_TL_BR) throw new NotSupportedException("Unsupported pixel order.");

        byte[] full = ReconstructFullByteStream(hdr);

        using var img = new Image<Rgb24>((int)hdr.Width, (int)hdr.Height);

        int idx = 0;
        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                img[x, y] = new Rgb24(full[idx], full[idx + 1], full[idx + 2]);
                idx += 3;
            }
        }

        img.Save(output);

        Console.WriteLine($"Decoded: {input}");
        Console.WriteLine($"Wrote:   {output}");
        return 0;
    }

    private static int DecimalCmd(string[] args)
    {
        if (args.Length < 3) return Usage("decimal requires <input.imgnum> <outputNumber.txt>.");

        string input = args[1];
        string output = args[2];

        Header hdr = ReadImgNum(input);
        BigInteger N = new BigInteger(hdr.Payload, isUnsigned: true, isBigEndian: true);

        File.WriteAllText(output, N.ToString());
        Console.WriteLine($"Wrote decimal integer to: {output}");
        return 0;
    }

    private static int InfoCmd(string[] args)
    {
        if (args.Length < 2) return Usage("info requires <input.imgnum>.");

        string input = args[1];
        Header hdr = ReadImgNum(input);

        string orderText = hdr.Order switch
        {
            ORDER_ROWMAJOR_TL_BR => "Top-left -> bottom-right (row-major)",
            _ => $"Unknown ({hdr.Order})"
        };

        int bitLen = GetBitLengthUnsignedBigEndian(hdr.Payload);
        int approxDigits = bitLen == 0 ? 1 : (int)Math.Floor((bitLen - 1) * 0.3010299956639812) + 1;

        byte[] full = ReconstructFullByteStream(hdr);
        string fullSha = ToHex(SHA256.HashData(full));
        string shortId = fullSha.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();

        Console.WriteLine("ImgNum info");
        Console.WriteLine($"File:         {input}");
        Console.WriteLine($"Version:      {VERSION}");
        Console.WriteLine($"Size:         {hdr.Width} x {hdr.Height}");
        Console.WriteLine($"Channels:     {hdr.Channels} (RGB8)");
        Console.WriteLine($"Order:        {orderText}");
        Console.WriteLine($"TotalBytes:   {hdr.TotalBytes}");
        Console.WriteLine($"PayloadBytes: {hdr.Payload.Length}");
        Console.WriteLine($"BitLength:    {bitLen}");
        Console.WriteLine($"Digits(~):    {approxDigits} (approx, integer payload only)");
        Console.WriteLine($"ID:           {shortId}");
        Console.WriteLine($"SHA256:       {fullSha}");
        return 0;
    }

    private static int HashCmd(string[] args)
    {
        if (args.Length < 2) return Usage("hash requires <input.imgnum>.");

        string input = args[1];
        Header hdr = ReadImgNum(input);

        byte[] full = ReconstructFullByteStream(hdr);
        string hex = ToHex(SHA256.HashData(full));
        string shortId = hex.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();

        Console.WriteLine("ImgNum hash");
        Console.WriteLine($"File:   {input}");
        Console.WriteLine($"ID:     {shortId}");
        Console.WriteLine($"SHA256: {hex}");
        return 0;
    }

    private static int VerifyCmd(string[] args)
    {
        if (args.Length < 3) return Usage("verify requires <input.imgnum> <imageToCompare>.");

        string imgnumPath = args[1];
        string imagePath = args[2];

        Header hdr = ReadImgNum(imgnumPath);

        if (hdr.Channels != 3) throw new NotSupportedException("Only RGB (3 channels) is supported.");
        if (hdr.Order != ORDER_ROWMAJOR_TL_BR) throw new NotSupportedException("Unsupported pixel order.");

        byte[] fromImgNum = ReconstructFullByteStream(hdr);
        byte[] hashA = SHA256.HashData(fromImgNum);

        using Image<Rgb24> img = Image.Load<Rgb24>(imagePath);

        if (img.Width != (int)hdr.Width || img.Height != (int)hdr.Height)
        {
            Console.WriteLine("VERIFY: NO MATCH");
            Console.WriteLine($"Reason: Dimension mismatch. imgnum={hdr.Width}x{hdr.Height}, image={img.Width}x{img.Height}");
            return 3;
        }

        int expectedBytes = checked(img.Width * img.Height * 3);
        byte[] fromImage = new byte[expectedBytes];
        int idx = 0;

        for (int y = 0; y < img.Height; y++)
        {
            for (int x = 0; x < img.Width; x++)
            {
                Rgb24 p = img[x, y];
                fromImage[idx++] = p.R;
                fromImage[idx++] = p.G;
                fromImage[idx++] = p.B;
            }
        }

        byte[] hashB = SHA256.HashData(fromImage);
        bool match = FixedTimeEquals(hashA, hashB);

        string hexA = ToHex(hashA);
        string hexB = ToHex(hashB);

        string idA = hexA.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();
        string idB = hexB.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();

        Console.WriteLine(match ? "VERIFY: MATCH" : "VERIFY: NO MATCH");
        Console.WriteLine($"IMGNUM ID:     {idA}");
        Console.WriteLine($"IMAGE  ID:     {idB}");
        Console.WriteLine($"IMGNUM SHA256: {hexA}");
        Console.WriteLine($"IMAGE  SHA256: {hexB}");

        return match ? 0 : 4;
    }

    private static int SealCmd(string[] args)
    {
        if (args.Length < 3) return Usage("seal requires <input.imgnum> <output.seal>.");

        string imgnumPath = args[1];
        string sealPath = args[2];

        Header hdr = ReadImgNum(imgnumPath);
        byte[] full = ReconstructFullByteStream(hdr);

        string shaHex = ToHex(SHA256.HashData(full));
        string shortId = shaHex.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();

        WriteSealFile(sealPath, imgnumPath, hdr, shortId, shaHex);

        Console.WriteLine("SEAL: CREATED");
        Console.WriteLine($"Imgnum: {imgnumPath}");
        Console.WriteLine($"Seal:   {sealPath}");
        Console.WriteLine($"ID:     {shortId}");
        Console.WriteLine($"SHA256: {shaHex}");
        return 0;
    }

    private static int CheckSealCmd(string[] args)
    {
        if (args.Length < 3) return Usage("checkseal requires <input.imgnum> <input.seal>.");

        string imgnumPath = args[1];
        string sealPath = args[2];

        var seal = ReadSeal(sealPath);

        if (!seal.TryGetValue("_header", out var header) || header != SEAL_HEADER)
            throw new InvalidDataException("Seal file has unknown header/version.");

        if (!seal.TryGetValue("sha256", out var expectedSha) || expectedSha.Length < 10)
            throw new InvalidDataException("Seal file missing sha256.");

        string expectedShort = seal.TryGetValue("shortid", out var sId) ? sId.Trim() : "";

        Header hdr = ReadImgNum(imgnumPath);
        byte[] full = ReconstructFullByteStream(hdr);

        string actualSha = ToHex(SHA256.HashData(full));
        string actualShort = actualSha.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();

        bool shaMatch = string.Equals(expectedSha.Trim(), actualSha, StringComparison.OrdinalIgnoreCase);
        bool shortMatch = string.IsNullOrWhiteSpace(expectedShort) || string.Equals(expectedShort, actualShort, StringComparison.OrdinalIgnoreCase);

        bool metaMatch = true;
        metaMatch &= MatchUInt(seal, "width", hdr.Width);
        metaMatch &= MatchUInt(seal, "height", hdr.Height);
        metaMatch &= MatchByte(seal, "channels", hdr.Channels);
        metaMatch &= MatchByte(seal, "order", hdr.Order);
        metaMatch &= MatchInt(seal, "totalBytes", hdr.TotalBytes);

        bool ok = shaMatch && metaMatch && shortMatch;

        Console.WriteLine(ok ? "CHECKSEAL: MATCH" : "CHECKSEAL: NO MATCH");
        Console.WriteLine($"Expected ID:    {(string.IsNullOrWhiteSpace(expectedShort) ? "(missing in seal)" : expectedShort)}");
        Console.WriteLine($"Actual   ID:    {actualShort}");
        Console.WriteLine($"Expected SHA256:{expectedSha}");
        Console.WriteLine($"Actual   SHA256:{actualSha}");

        if (!metaMatch)
            Console.WriteLine("Metadata mismatch detected (width/height/channels/order/totalBytes).");

        if (!shortMatch)
            Console.WriteLine("shortId mismatch detected.");

        return ok ? 0 : 4;
    }

    // NEW: batchseal <folder> [--recursive] [--index <path>]
    private static int BatchSealCmd(string[] args)
    {
        if (args.Length < 2) return Usage("batchseal requires <folder> [--recursive] [--index <path>].");

        string folder = args[1];
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Folder not found: {folder}");

        bool recursive = false;
        string? indexPath = null;

        for (int i = 2; i < args.Length; i++)
        {
            string a = args[i];
            if (string.Equals(a, "--recursive", StringComparison.OrdinalIgnoreCase))
            {
                recursive = true;
                continue;
            }

            if (string.Equals(a, "--index", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length) return Usage("batchseal: --index requires a path.");
                indexPath = args[i + 1];
                i++;
                continue;
            }

            return Usage($"batchseal: unknown option '{a}'.");
        }

        if (indexPath == null)
            indexPath = Path.Combine(folder, "imgnum_index.csv");

        var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        int count = 0;
        int ok = 0;
        int fail = 0;

        using var sw = new StreamWriter(indexPath, false, System.Text.Encoding.UTF8);
        sw.WriteLine("file,id,sha256,width,height,totalBytes,seal");

        foreach (var file in Directory.EnumerateFiles(folder, "*.imgnum", opt))
        {
            count++;
            try
            {
                Header hdr = ReadImgNum(file);
                byte[] full = ReconstructFullByteStream(hdr);

                string shaHex = ToHex(SHA256.HashData(full));
                string shortId = shaHex.Substring(0, SHORT_ID_HEX_LEN).ToUpperInvariant();

                string sealPath = file + ".seal";
                WriteSealFile(sealPath, file, hdr, shortId, shaHex);

                string rel = MakeRelativePathSafe(folder, file);
                string relSeal = MakeRelativePathSafe(folder, sealPath);

                sw.WriteLine(
                    $"{Csv(rel)},{Csv(shortId)},{Csv(shaHex)},{hdr.Width},{hdr.Height},{hdr.TotalBytes},{Csv(relSeal)}"
                );

                ok++;
            }
            catch (Exception ex)
            {
                fail++;
                string rel = MakeRelativePathSafe(folder, file);
                sw.WriteLine($"{Csv(rel)},{Csv("")},{Csv("ERROR")},{Csv(ex.Message.Replace("\r", " ").Replace("\n", " "))},,,");
            }
        }

        Console.WriteLine("BATCHSEAL: DONE");
        Console.WriteLine($"Folder:     {folder}");
        Console.WriteLine($"Recursive:  {recursive}");
        Console.WriteLine($"Index:      {indexPath}");
        Console.WriteLine($"Found:      {count}");
        Console.WriteLine($"Sealed:     {ok}");
        Console.WriteLine($"Failed:     {fail}");
        return fail == 0 ? 0 : 5;
    }

    // ---------- Core I/O ----------

    private static void WriteImgNum(string path, uint w, uint h, byte channels, byte order, ulong totalBytes, ulong payloadLen, byte[] payload)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write(MAGIC);
        bw.Write(VERSION);
        bw.Write(order);
        bw.Write(channels);
        bw.Write((byte)0);

        WriteU32BE(bw, w);
        WriteU32BE(bw, h);
        WriteU64BE(bw, totalBytes);
        WriteU64BE(bw, payloadLen);

        bw.Write(payload, 0, payload.Length);
    }

    private static Header ReadImgNum(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);

        byte[] magic = br.ReadBytes(6);
        if (magic.Length != 6 || !EqualBytes(magic, MAGIC))
            throw new InvalidDataException("Bad magic.");

        byte version = br.ReadByte();
        if (version != VERSION)
            throw new InvalidDataException($"Unsupported version: {version}");

        byte order = br.ReadByte();
        byte channels = br.ReadByte();
        br.ReadByte();

        uint w = ReadU32BE(br);
        uint h = ReadU32BE(br);

        ulong totalBytesU64 = ReadU64BE(br);
        ulong payloadLen = ReadU64BE(br);

        if (totalBytesU64 > int.MaxValue)
            throw new InvalidDataException("Image too large for this build (array length limit).");

        long remaining = fs.Length - fs.Position;
        if ((long)payloadLen > remaining)
            throw new InvalidDataException("Payload length invalid.");

        byte[] payload = br.ReadBytes((int)payloadLen);

        return new Header
        {
            Width = w,
            Height = h,
            Channels = channels,
            Order = order,
            TotalBytes = (int)totalBytesU64,
            Payload = payload
        };
    }

    private static byte[] ReconstructFullByteStream(Header hdr)
    {
        if (hdr.Payload.Length > hdr.TotalBytes)
            throw new InvalidDataException("Payload longer than total bytes.");

        byte[] full = new byte[hdr.TotalBytes];
        int pad = hdr.TotalBytes - hdr.Payload.Length;
        Buffer.BlockCopy(hdr.Payload, 0, full, pad, hdr.Payload.Length);
        return full;
    }

    // ---------- Seal helpers ----------

    private static void WriteSealFile(string sealPath, string imgnumPath, Header hdr, string shortId, string shaHex)
    {
        var createdUtc = DateTime.UtcNow.ToString("O");

        using var sw = new StreamWriter(sealPath, false, System.Text.Encoding.UTF8);
        sw.WriteLine(SEAL_HEADER);
        sw.WriteLine($"tag={SEAL_TAG}");
        sw.WriteLine($"createdUtc={createdUtc}");
        sw.WriteLine($"shortId={shortId}");
        sw.WriteLine($"sha256={shaHex}");
        sw.WriteLine($"width={hdr.Width}");
        sw.WriteLine($"height={hdr.Height}");
        sw.WriteLine($"channels={hdr.Channels}");
        sw.WriteLine($"order={hdr.Order}");
        sw.WriteLine($"totalBytes={hdr.TotalBytes}");
        sw.WriteLine($"sourceImgnum={Path.GetFileName(imgnumPath)}");
    }

    private static System.Collections.Generic.Dictionary<string, string> ReadSeal(string path)
    {
        var dict = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var sr = new StreamReader(path, System.Text.Encoding.UTF8);
        string? first = sr.ReadLine();
        if (first == null) throw new InvalidDataException("Empty seal file.");

        dict["_header"] = first.Trim();

        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith("#")) continue;

            int eq = line.IndexOf('=');
            if (eq <= 0) continue;

            string key = line.Substring(0, eq).Trim();
            string val = line.Substring(eq + 1).Trim();
            dict[key] = val;
        }

        return dict;
    }

    private static bool MatchUInt(System.Collections.Generic.Dictionary<string, string> d, string key, uint actual)
    {
        if (!d.TryGetValue(key, out var s)) return true;
        return uint.TryParse(s, out var exp) && exp == actual;
    }

    private static bool MatchByte(System.Collections.Generic.Dictionary<string, string> d, string key, byte actual)
    {
        if (!d.TryGetValue(key, out var s)) return true;
        return byte.TryParse(s, out var exp) && exp == actual;
    }

    private static bool MatchInt(System.Collections.Generic.Dictionary<string, string> d, string key, int actual)
    {
        if (!d.TryGetValue(key, out var s)) return true;
        return int.TryParse(s, out var exp) && exp == actual;
    }

    // ---------- CSV helpers ----------

    private static string Csv(string s)
    {
        // Minimal CSV escape: quote if needed, double quotes inside
        bool need = s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0 || s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0;
        if (!need) return s;

        string t = s.Replace("\"", "\"\"");
        return $"\"{t}\"";
    }

    private static string MakeRelativePathSafe(string baseFolder, string fullPath)
    {
        try
        {
            return Path.GetRelativePath(baseFolder, fullPath);
        }
        catch
        {
            return fullPath;
        }
    }

    // ---------- BE integer helpers ----------

    private static void WriteU32BE(BinaryWriter bw, uint v)
    {
        bw.Write((byte)(v >> 24));
        bw.Write((byte)(v >> 16));
        bw.Write((byte)(v >> 8));
        bw.Write((byte)v);
    }

    private static void WriteU64BE(BinaryWriter bw, ulong v)
    {
        bw.Write((byte)(v >> 56));
        bw.Write((byte)(v >> 48));
        bw.Write((byte)(v >> 40));
        bw.Write((byte)(v >> 32));
        bw.Write((byte)(v >> 24));
        bw.Write((byte)(v >> 16));
        bw.Write((byte)(v >> 8));
        bw.Write((byte)v);
    }

    private static uint ReadU32BE(BinaryReader br)
    {
        uint b1 = br.ReadByte();
        uint b2 = br.ReadByte();
        uint b3 = br.ReadByte();
        uint b4 = br.ReadByte();
        return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
    }

    private static ulong ReadU64BE(BinaryReader br)
    {
        ulong b1 = br.ReadByte();
        ulong b2 = br.ReadByte();
        ulong b3 = br.ReadByte();
        ulong b4 = br.ReadByte();
        ulong b5 = br.ReadByte();
        ulong b6 = br.ReadByte();
        ulong b7 = br.ReadByte();
        ulong b8 = br.ReadByte();
        return (b1 << 56) | (b2 << 48) | (b3 << 40) | (b4 << 32) | (b5 << 24) | (b6 << 16) | (b7 << 8) | b8;
    }

    private static bool EqualBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }

    private static int GetBitLengthUnsignedBigEndian(byte[] be)
    {
        if (be == null || be.Length == 0) return 0;
        if (be.Length == 1 && be[0] == 0) return 0;

        byte first = be[0];
        int lz = LeadingZeroCount8(first);
        int bitsInFirst = 8 - lz;
        return (be.Length - 1) * 8 + bitsInFirst;
    }

    private static int LeadingZeroCount8(byte b)
    {
        int count = 0;
        for (int i = 7; i >= 0; i--)
        {
            if (((b >> i) & 1) == 0) count++;
            else break;
        }
        return count;
    }

    private static string ToHex(byte[] bytes)
    {
        char[] c = new char[bytes.Length * 2];
        const string hex = "0123456789abcdef";
        for (int i = 0; i < bytes.Length; i++)
        {
            c[i * 2] = hex[bytes[i] >> 4];
            c[i * 2 + 1] = hex[bytes[i] & 0x0F];
        }
        return new string(c);
    }
}
