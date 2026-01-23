# ImgNum
> **Attribution:** Created by **Peter Härje (“Endarker”)** (Idea) and **Algot Rydberg (“AggE”)** (Programmer).  
> This project ships with `NOTICE` for permanent credit—keep it with redistributions (Apache-2.0).  
> *Frankenstein Junkpile Transmission: memory anchored as number.*

**ImgNum** mappar en bild till *ett enda heltal* (lagrat effektivt) och kan sedan återskapa bilden **exakt** från samma tal.  
Detta är en bijektion mellan en bilds pixeldata (RGB8) och ett “bildnummer” representerat som ett stort heltal i bas 256 (big-endian).

---

## Credits
- **Programmer:** Algot Rydberg “AggE”
- **Idea:** Peter Härje “Endarker”

---

## Vad gör programmet?
ImgNum läser en bild, skannar pixlar i ordningen **övre vänster → nedre höger** (radvis), och skriver ut en `.imgnum`-fil:
- En liten header med metadata (bredd, höjd, kanaler, ordning, längder)
- En payload som är bilden tolkad som ett enda stort heltal (big-endian)

Sedan kan `.imgnum`-filen avkodas tillbaka till exakt samma RGB-bild.

---

## Pixelordning
- Row-major: `y = 0..H-1`, `x = 0..W-1`
- Start: `(0,0)` (övre vänster)
- Slut: `(W-1, H-1)` (nedre höger)

---

## Varför binärt istället för decimal text?
Decimalform blir enorm (miljontals siffror). Binär form är effektiv och exakt.  
För en 1920×1080 RGB-bild blir rådatat **6 220 800 bytes** (~6.2 MB) plus en liten header.

> Ja, det är fortfarande “ett nummer” — men lagrat smart, inte som ett gigantiskt text-tal.

---

## Short ID (16 hex)
Programmet skriver även ut en kort “fingerprint”:
- **shortId = första 16 hex-tecken** av SHA-256 (64-bit)

Exempel:
`F2DE50F7B25F64EC`

Det är perfekt för metadata, filnamn, beskrivningar och “proof-of-origin”.  
För full vetenskaplig verifiering används hela SHA-256.

---

## Kommandon
- `encode` : bild → `.imgnum`
- `decode` : `.imgnum` → bild
- `hash` : skriver `shortId` + SHA-256
- `info` : metadata + bitlängd/digits approx + hash/ID
- `verify` : jämför `.imgnum` mot en bild (byte-exakt RGB)
- `seal` : skapar `.seal`-fil (människoläsbar stämpel)
- `checkseal` : kontrollerar `.imgnum` mot `.seal`
- `batchseal` : skapar seals + CSV-index för alla `.imgnum` i en mapp
- `decimal` : (valfritt) skriver ut talet i decimal text (**kan bli extremt stort**)

---

## Exempel

### Encode / decode
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
Verify
dotnet run -c Release -- verify output.imgnum input.png
Seal / checkseal
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
Hash / info
dotnet run -c Release -- hash output.imgnum
dotnet run -c Release -- info output.imgnum
Batchseal (skapar seals + index CSV)
dotnet run -c Release -- batchseal C:\path\to\folder
dotnet run -c Release -- batchseal C:\path\to\folder --recursive
dotnet run -c Release -- batchseal C:\path\to\folder --index C:\path\to\imgnum_index.csv
Decimal (WARNING)
dotnet run -c Release -- decimal output.imgnum number.txt
Build
Kör dotnet build i mappen där ImgNum.csproj finns (inte i bin\Release\...):

dotnet build -c Release
Publish Windows exe
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true
Run (published exe)
ImgNum.exe encode input.png output.imgnum
ImgNum.exe decode output.imgnum restored.png
ImgNum.exe verify output.imgnum input.png
ImgNum.exe seal output.imgnum output.seal
ImgNum.exe checkseal output.imgnum output.seal
ImgNum.exe batchseal . --recursive
File format: .imgnum
Contains:

MAGIC + version

pixel order + channels

width/height

total byte count required for exact reconstruction (preserves leading zeros)

payload length + payload (big-endian integer bytes)

Frankenstein Junkpile Transmission (fiction lore)
This tool is part of the Frankenstein Junkpile fragments: technical artifacts “leaking” backward in time.

In the future, humanity shifts beyond purely biological bodies. Some become flesh machines, others metal machines, and many evolve further into energy-forms where identity lives as patterns, frequencies, and signals.

When the body is no longer the primary container, memory becomes fragile:

Who am I?
Who was I?
How do I prove continuity?

ImgNum is one answer: a method to lock a “life-frame” into a number — not to reduce meaning to data, but to preserve a thread through changing forms.

We sent it here as a tool.
You use it now as code.
They use it later as memory.
