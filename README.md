# ImgNum
> **Attribution:** Created by **Peter Härje (“Endarker”)** (Idea) and **Algot Rydberg (“AggE”)** (Programmer).  
> This project ships with `NOTICE` for permanent credit—keep it with redistributions (Apache-2.0).  
> *Frankenstein Junkpile Transmission: memory anchored as number.*
> **Tagline:** *ImgNum: a Junkpile fragment for encoding life-frames. Svelatina isn’t a language… yet.*


## Languages
- [Svenska](#readme-svenska)
- [English](#readme-english)
- [Français](#readme-français)
- [Deutsch](#readme-deutsch)
- [Español](#readme-español)
- [中文（简体中文—普通话）](#readme-中文简体中文普通话)
- [Latine](#readme-latine--neo-latinitas)
- [Sveottal] (#readme-Sveottal)


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

README (English)
# README (English)

**ImgNum** maps an image to **one single integer** (stored efficiently) and can reconstruct the image **exactly** from that number.  
It is a bijection between fixed-size **RGB8 pixel data** and a **base-256 big-endian unsigned integer**, stored in `*.imgnum`.

### Credits
- **Programmer:** Algot Rydberg “AggE”
- **Idea:** Peter Härje “Endarker”

### Pixel order
Top-left → bottom-right (row-major): `y=0..H-1`, `x=0..W-1`.

### Short ID (16 hex)
`shortId` = first 16 hex chars of SHA-256 (64-bit fingerprint). Use it for filenames/metadata.

### Commands
- `encode` : image → `.imgnum`
- `decode` : `.imgnum` → image
- `hash` : prints `shortId` + SHA-256
- `info` : metadata + size + hash/ID
- `verify` : compares `.imgnum` to an image (byte-exact RGB stream)
- `seal` : creates a `.seal` file (human-readable stamp)
- `checkseal` : verifies `.imgnum` matches a `.seal`
- `batchseal` : seals all `*.imgnum` in a folder + writes `imgnum_index.csv`
- `decimal` : writes the integer as decimal text (can be enormous)

### Examples
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive

Build / Publish

Build from the folder containing ImgNum.csproj:

dotnet build -c Release


Publish Windows single-file exe:

dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

.imgnum format (summary)

Header includes width/height/order/channels and TOTAL_BYTES (preserves leading zeros), plus minimal big-endian integer payload.


---

## README (Français)

```markdown
# README (Français)

**ImgNum** associe une image à **un seul entier** (stocké efficacement) et peut reconstruire l’image **à l’identique** depuis ce nombre.  
C’est une bijection entre des pixels **RGB8** (taille fixe) et un **entier non signé en base 256** (big-endian), stocké dans `*.imgnum`.

### Crédits
- **Programmeur :** Algot Rydberg “AggE”
- **Idée :** Peter Härje “Endarker”

### Ordre des pixels
Haut-gauche → bas-droite (row-major) : `y=0..H-1`, `x=0..W-1`.

### Short ID (16 hex)
`shortId` = 16 premiers caractères hex de SHA-256 (empreinte 64-bit).

### Commandes
- `encode` : image → `.imgnum`
- `decode` : `.imgnum` → image
- `hash` : affiche `shortId` + SHA-256
- `info` : métadonnées + taille + hash/ID
- `verify` : compare `.imgnum` à une image (RGB exact)
- `seal` : crée un fichier `.seal` (tampon lisible)
- `checkseal` : vérifie `.imgnum` contre `.seal`
- `batchseal` : scelle tous les `*.imgnum` + écrit `imgnum_index.csv`
- `decimal` : écrit l’entier en texte décimal (peut être énorme)

### Exemples
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive

Compiler / Publier
dotnet build -c Release
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

Format .imgnum (résumé)

L’en-tête contient largeur/hauteur/ordre/canaux et TOTAL_BYTES (préserve les zéros de tête) + payload minimal (entier big-endian).


---

## README (Deutsch)

```markdown
# README (Deutsch)

**ImgNum** ordnet einem Bild **eine einzige ganze Zahl** zu (effizient gespeichert) und kann das Bild **exakt** daraus rekonstruieren.  
Es ist eine Bijektion zwischen **RGB8-Pixeldaten** (feste Größe) und einer **Base-256 big-endian unsigned Integerzahl**, gespeichert als `*.imgnum`.

### Credits
- **Programmierer:** Algot Rydberg “AggE”
- **Idee:** Peter Härje “Endarker”

### Pixelreihenfolge
Oben-links → unten-rechts (row-major): `y=0..H-1`, `x=0..W-1`.

### Short ID (16 hex)
`shortId` = erste 16 Hex-Zeichen von SHA-256 (64-bit Fingerprint).

### Befehle
- `encode` : Bild → `.imgnum`
- `decode` : `.imgnum` → Bild
- `hash` : `shortId` + SHA-256
- `info` : Metadaten + Größe + Hash/ID
- `verify` : vergleicht `.imgnum` mit einem Bild (byte-exakt)
- `seal` : erstellt `.seal` (lesbarer Stempel)
- `checkseal` : prüft `.imgnum` gegen `.seal`
- `batchseal` : seal für alle `*.imgnum` + `imgnum_index.csv`
- `decimal` : schreibt Zahl als Dezimaltext (kann riesig sein)

### Beispiele
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive

Build / Publish
dotnet build -c Release
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

.imgnum Format (Kurz)

Header enthält u.a. TOTAL_BYTES (erhält führende Nullen) + minimalen big-endian Integer-Payload.


---

## README (Español)

```markdown
# README (Español)

**ImgNum** asigna a una imagen **un solo número entero** (almacenado eficientemente) y puede reconstruir la imagen **exactamente** desde ese número.  
Es una biyección entre píxeles **RGB8** (tamaño fijo) y un **entero sin signo en base 256** (big-endian), guardado como `*.imgnum`.

### Créditos
- **Programador:** Algot Rydberg “AggE”
- **Idea:** Peter Härje “Endarker”

### Orden de píxeles
Arriba-izquierda → abajo-derecha (row-major): `y=0..H-1`, `x=0..W-1`.

### Short ID (16 hex)
`shortId` = primeros 16 caracteres hex de SHA-256 (huella 64-bit).

### Comandos
- `encode` : imagen → `.imgnum`
- `decode` : `.imgnum` → imagen
- `hash` : imprime `shortId` + SHA-256
- `info` : metadatos + tamaño + hash/ID
- `verify` : compara `.imgnum` con una imagen (RGB exacto por bytes)
- `seal` : crea `.seal` (sello legible)
- `checkseal` : verifica `.imgnum` contra `.seal`
- `batchseal` : sella todos los `*.imgnum` + `imgnum_index.csv`
- `decimal` : escribe el entero en texto decimal (puede ser enorme)

### Ejemplos
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive

Compilar / Publicar
dotnet build -c Release
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

Formato .imgnum (resumen)

El header incluye TOTAL_BYTES (preserva ceros iniciales) y un payload mínimo como entero big-endian.


---

## README (中文 / 简体中文 — 普通话)

```markdown
# README（中文 / 简体中文 — 普通话）

**ImgNum** 可以把一张图片映射为**一个整数**（高效存储），并能从该整数**完全还原**图片。  
它是固定尺寸 **RGB8 像素数据**与 **256 进制无符号大整数（big-endian）**之间的双射，存储在 `*.imgnum` 中。

### 致谢
- **程序编写：** Algot Rydberg “AggE”
- **创意提出：** Peter Härje “Endarker”

### 像素顺序
左上 → 右下（按行扫描）：`y=0..H-1`, `x=0..W-1`。

### Short ID（16 hex）
`shortId` = SHA-256 的前 16 个十六进制字符（64-bit 指纹），适合放在文件名/元数据中。

### 命令
- `encode`：图片 → `.imgnum`
- `decode`：`.imgnum` → 图片
- `hash`：输出 `shortId` + SHA-256
- `info`：元数据 + 尺寸 + hash/ID
- `verify`：对比 `.imgnum` 与图片（按 RGB 字节严格一致）
- `seal`：生成 `.seal`（可读“印章”文件）
- `checkseal`：校验 `.imgnum` 是否匹配 `.seal`
- `batchseal`：批量生成 seal + 输出 `imgnum_index.csv`
- `decimal`：将整数写成十进制文本（可能非常巨大）

### 示例
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive

编译 / 发布
dotnet build -c Release
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

.imgnum 格式（摘要）

头部包含 TOTAL_BYTES（保留前导零）以及最小 big-endian 整数 payload。


---

## README (Latine / Neo-Latinitas)

```markdown
# README (Latine / Neo-Latinitas)

**ImgNum** imaginem in **unum numerum integrum** (compendiose servatum) convertit, atque imaginem **omnino restituit** ex eodem numero.  
Haec est bijectio inter data pixelorum **RGB8** (magnitudo fixa) et **integerem unsigned in basi 256** (big-endian), servatum in `*.imgnum`.

### Auctores
- **Programmer:** Algot Rydberg “AggE”
- **Idea:** Peter Härje “Endarker”

### Ordo pixelorum
Summus sinister → imus dexter (per lineas): `y=0..H-1`, `x=0..W-1`.

### Short ID (16 hex)
`shortId` = prima 16 signa hex ex SHA-256 (vestigium 64-bit).

### Praecepta
- `encode` : imago → `.imgnum`
- `decode` : `.imgnum` → imago
- `hash` : `shortId` + SHA-256
- `info` : metadata + magnitudo + hash/ID
- `verify` : comparat `.imgnum` cum imagine (byte-exacte)
- `seal` : facit `.seal` (sigillum legibile)
- `checkseal` : probat `.imgnum` contra `.seal`
- `batchseal` : sigilla omnibus `*.imgnum` + `imgnum_index.csv`
- `decimal` : numerum in decimali scribit (saepe immensum)

### Exempla
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive

Aedificare / Publicare
dotnet build -c Release
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

Forma .imgnum (breviarium)

Caput continet TOTAL_BYTES (servat zera praevia) et payload minimalem integeris big-endian.

#readme-Sveottal

> **Regula Svelatina:** Konsonanta ex Svenska; vokala ex Latina.  
> (Nula Å/Ä/Ö. Vokala: a e i o u y, et si vis: ā ē ī ō ū ȳ.)

## Brevis Deskriptio
**ImgNum** mappār imāgo til yn singular nymērūs, et restārār imāgo ex eodem nymērūs exakte.  
Iste est bijektiōn inter RGB8 pixēla-stream et yn magnus bas-256 nymērūs (big-endian), servāt in `*.imgnum`.

## Kredita
- **Programmer:** Algot Rydberg “AggE”
- **Idea:** Peter Harje “Endarker”

## Piksel-Ordo
Ordo: top-left → bottom-right, row-major: `y=0..H-1`, `x=0..W-1`.  
Init: `(0,0)`; fin: `(W-1, H-1)`.

## ShortId (XVI hex)
`shortId` = prima XVI hex ex SHA-256 (vestigium 64-bit).  
Usa til fil-nomena, metadata, et “prova-origo”.

## Kommando-Lex
- `encode` : imāgo → `.imgnum`
- `decode` : `.imgnum` → imāgo
- `hash` : skrivar shortId + SHA-256
- `info` : metada + magnitudo + hash/ID
- `verify` : verifīār `.imgnum` vs imāgo (byte-exakt RGB)
- `seal` : skapar `.seal` (legibila stamp)
- `checkseal` : kontrollār `.imgnum` vs `.seal`
- `batchseal` : sigillār omnia `*.imgnum` in folder + `imgnum_index.csv`
- `decimal` : skrivar nymērūs in decimal-text (potest esse enormis)

## Exempla
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- verify output.imgnum input.png
dotnet run -c Release -- seal output.imgnum output.seal
dotnet run -c Release -- checkseal output.imgnum output.seal
dotnet run -c Release -- batchseal . --recursive
