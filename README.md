# ImgNum
> **Attribution:** Created by **Peter Härje (“Endarker”)** (Idea) and **Algot Rydberg (“AggE”)** (Programmer).  
> This project ships with `NOTICE` for permanent credit—keep it with redistributions (Apache-2.0).  
> *Frankenstein Junkpile Transmission: memory anchored as number.*

**ImgNum** mappar en bild till *ett enda heltal* och kan sedan återskapa bilden exakt från samma tal.
Detta är en bijektion mellan en bilds pixeldata (RGB8) och ett “bildnummer” representerat som ett stort heltal i bas 256.

## Credits
- **Programmer:** Algot Rydberg “AggE”
- **Idea:** Peter Härje “Endarker”

## Vad gör programmet?
ImgNum läser en bild, skannar pixlar i ordningen **övre vänster → nedre höger** (radvis), och skriver ut en `.imgnum`-fil:
- En liten header med metadata (bredd, höjd, kanaler, ordning).
- En payload som är bilden tolkad som ett enda stort heltal (big-endian).

Sedan kan `.imgnum`-filen avkodas tillbaka till exakt samma RGB-bild.

## Pixelordning
- Row-major: `y = 0..H-1`, `x = 0..W-1`
- Start: `(0,0)` (övre vänster)
- Slut: `(W-1, H-1)` (nedre höger)

## Varför binärt istället för decimal text?
Decimalform blir enorm (miljontals siffror). Binär form är effektiv och exakt.
För en 1920×1080 RGB-bild blir rådatat **6 220 800 bytes** (~6.2 MB) plus en liten header.

> Ja, det är fortfarande “ett nummer” — men lagrat smart, inte som ett gigantiskt text-tal.

## Kommandon
- `encode` : bild → `.imgnum`
- `decode` : `.imgnum` → bild
- `decimal`: (valfritt) skriver ut talet i decimal text (kan bli extremt stort)

### Exempel
```bash
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- decimal output.imgnum number.txt

README (English)
ImgNum

ImgNum maps an image to one single integer and can recreate the image exactly from that integer.
It is a bijection between RGB pixel data and a “picture number”, represented as a huge base-256 integer.

Credits

Programmer: Algot Rydberg “AggE”

Idea: Peter Härje “Endarker”

What does it do?

ImgNum reads an image, scans pixels top-left → bottom-right (row-major), and writes a .imgnum file that contains:

A small header (width, height, channels, order, lengths)

A payload: the full RGB byte stream interpreted as one big-endian unsigned integer

Then it can decode that .imgnum file back into the original image.

Pixel order

Row-major:

y = 0..H-1

x = 0..W-1
Start: (0,0) (top-left)
End: (W-1, H-1) (bottom-right)

Why binary instead of decimal text?

Decimal text becomes absurdly large (millions of digits). Binary is compact and exact.
A 1920×1080 RGB image becomes 6,220,800 bytes (~6.2 MB) plus a tiny header.

Yes, it is still “a number” — stored efficiently, not as a giant decimal string.

Commands

encode : image → .imgnum

decode : .imgnum → image

decimal: optional, writes the integer as decimal text (can be enormous)

Examples
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- decimal output.imgnum number.txt

Build
dotnet build -c Release

Publish a Windows exe
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true


Run:

ImgNum.exe encode input.png output.imgnum
ImgNum.exe decode output.imgnum restored.png

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

README (Français)
ImgNum

ImgNum associe une image à un seul entier et peut recréer l’image à l’identique à partir de cet entier.
C’est une bijection entre des pixels RGB et un “numéro d’image”, représenté comme un entier géant en base 256.

Crédits

Programmeur : Algot Rydberg “AggE”

Idée : Peter Härje “Endarker”

À quoi ça sert ?

ImgNum lit une image, scanne les pixels en haut-gauche → bas-droite (par lignes), puis écrit un fichier .imgnum contenant :

Un petit en-tête (largeur, hauteur, canaux, ordre, longueurs)

Un payload : tout le flux d’octets RGB interprété comme un entier non signé big-endian

Ensuite, il peut décoder ce fichier .imgnum et reconstruire l’image originale.

Ordre des pixels

Par lignes (row-major) :

y = 0..H-1

x = 0..W-1
Début : (0,0) (haut-gauche)
Fin : (W-1, H-1) (bas-droite)

Pourquoi binaire plutôt que texte décimal ?

Le texte décimal devient gigantesque (des millions de chiffres). Le binaire est compact et exact.
Une image 1920×1080 en RGB correspond à 6 220 800 octets (~6,2 MB) + un petit en-tête.

Oui, c’est toujours “un nombre” — simplement stocké efficacement.

Commandes

encode : image → .imgnum

decode : .imgnum → image

decimal: optionnel, écrit l’entier en texte décimal (peut être énorme)

Exemples
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- decimal output.imgnum number.txt

Compilation
dotnet build -c Release

Publier un exe Windows
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

Format de fichier : .imgnum

Contient :

MAGIC + version

ordre des pixels + canaux

largeur/hauteur

nombre total d’octets requis pour reconstruire exactement (conserve les zéros de tête)

longueur du payload + payload (octets big-endian)

Transmission Frankenstein Junkpile (fiction)

Cet outil fait partie des fragments Frankenstein Junkpile : des artefacts techniques “reçus” depuis le futur.

Dans l’avenir, l’humanité dépasse le biologique. Certains deviennent des machines de chair, d’autres des machines de métal, et beaucoup évoluent vers des formes d’énergie où l’identité est un motif, une fréquence, un signal.

Quand le corps n’est plus le contenant principal, la mémoire devient fragile.
ImgNum est une réponse : enfermer une “trame de vie” dans un nombre, pour garder un fil à travers les formes.

README (Deutsch)
ImgNum

ImgNum ordnet einem Bild eine einzige ganze Zahl zu und kann das Bild exakt aus dieser Zahl rekonstruieren.
Es ist eine Bijektion zwischen RGB-Pixeldaten und einer “Bildnummer”, dargestellt als riesige Zahl in Basis 256.

Credits

Programmierer: Algot Rydberg “AggE”

Idee: Peter Härje “Endarker”

Was macht es?

ImgNum liest ein Bild, scannt Pixel oben-links → unten-rechts (zeilenweise) und schreibt eine .imgnum-Datei:

Kleiner Header (Breite, Höhe, Kanäle, Reihenfolge, Längen)

Payload: der komplette RGB-Byte-Strom als eine big-endian, unsigned Integerzahl

Dann kann .imgnum wieder in das Originalbild dekodiert werden.

Pixelreihenfolge

Row-major:

y = 0..H-1

x = 0..W-1
Start: (0,0) (oben-links)
Ende: (W-1, H-1) (unten-rechts)

Warum binär statt dezimaler Text?

Dezimaltext wird extrem groß (Millionen Ziffern). Binär ist kompakt und exakt.
Ein 1920×1080 RGB-Bild entspricht 6.220.800 Bytes (~6,2 MB) plus kleinem Header.

Befehle

encode : Bild → .imgnum

decode : .imgnum → Bild

decimal: optional, schreibt die Zahl als Dezimaltext (kann riesig sein)

Beispiele
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- decimal output.imgnum number.txt

Build
dotnet build -c Release

Windows-Exe veröffentlichen
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

Dateiformat: .imgnum

Enthält:

MAGIC + Version

Pixelreihenfolge + Kanäle

Breite/Höhe

Gesamtanzahl Bytes für exakte Rekonstruktion (erhält führende Nullen)

Payload-Länge + Payload (big-endian Integer-Bytes)

Frankenstein Junkpile Transmission (Fiction)

Dieses Tool ist ein Fragment aus Frankenstein Junkpile: Technik aus einer Zukunft, die in unsere Zeit “durchsickert”.

In der Zukunft werden Menschen zu Fleischmaschinen, Metallmaschinen und schließlich zu Energieformen – Identität als Muster und Signal.
Wenn der Körper nicht mehr Träger ist, wird Erinnerung verletzlich. ImgNum ist eine Methode, einen “Lebens-Frame” als Zahl zu verankern.

README (中文 / 简体中文 — 普通话)
ImgNum

ImgNum 可以把一张图片映射为一个整数，并且能够从这个整数完全还原出原图。
它建立了 RGB 像素数据与“图片编号”（一个巨大的 256 进制整数）之间的**一一对应（双射）**关系。

致谢

程序编写： Algot Rydberg “AggE”

创意提出： Peter Härje “Endarker”

它做什么？

ImgNum 读取图片后按 左上 → 右下（按行扫描）遍历像素，并输出 .imgnum 文件，包含：

小型头信息（宽、高、通道数、扫描顺序、长度信息）

数据区：将完整 RGB 字节流解释为一个大端序（big-endian）的无符号整数

然后可以将 .imgnum 解码回原始图片。

像素扫描顺序

按行（row-major）：

y = 0..H-1

x = 0..W-1
起点：(0,0)（左上）
终点：(W-1, H-1)（右下）

为什么用二进制而不是十进制文本？

十进制文本会巨大到不可用（可能上百万位数字）。二进制更紧凑且精确。
1920×1080 的 RGB 图像约等于 6,220,800 字节（约 6.2 MB）+ 少量头信息。

命令

encode：图片 → .imgnum

decode：.imgnum → 图片

decimal：可选，将整数写成十进制文本（可能非常巨大）

示例
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- decimal output.imgnum number.txt

编译
dotnet build -c Release

发布 Windows 可执行文件
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true

文件格式：.imgnum

包含：

MAGIC + 版本号

扫描顺序 + 通道数

宽/高

用于精确还原的总字节数（保留前导零）

数据长度 + 数据（大端序整数的字节表示）

Frankenstein Junkpile 传输（科幻设定）

本工具属于 Frankenstein Junkpile 的“碎片”：来自未来、泄漏到现在的技术残片。

在未来，人类逐步超越纯粹的肉体：有人变成 flesh machines（血肉机器），有人变成 metal machines（金属机器），还有人进化为 能量形态，身份以模式、频率与信号存在。
当身体不再是唯一载体，记忆就变得脆弱。ImgNum 提供一种方法：把“生命片段”锚定成一个数字，以穿越形态的变化。

README (Latine / Neo-Latinitas)
ImgNum

ImgNum imaginem in unum numerum integrum convertit, atque imaginem omnino restituit ex eodem numero.
Haec est bijectio inter data pixelorum RGB et “numerum imaginis”, repraesentatum ut numerus ingens in basi 256.

Auctores

Programmer: Algot Rydberg “AggE”

Idea: Peter Härje “Endarker”

Quid facit?

ImgNum imaginem legit, pixels ordine a summo sinistro ad imum dextrum (per lineas) percurrit, et .imgnum scribit:

caput breve (latitudo, altitudo, canalis, ordo, longitudines)

corpus: totus fluxus RGB ut integer unsigned big-endian interpretatus

Deinde .imgnum in imaginem originalem decodari potest.

Ordo pixelorum

Row-major:

y = 0..H-1

x = 0..W-1
Initium: (0,0) (summus sinister)
Finis: (W-1, H-1) (imus dexter)

Cur binarium, non decimalis textus?

Textus decimalis fit immensum (multae milliones notarum). Binarium est compendiosum et exactum.
Imago 1920×1080 RGB est circa 6,220,800 bytes + caput parvum.

Praecepta

encode : imago → .imgnum

decode : .imgnum → imago

decimal: optativum, numerum in decimali scribit (saepe nimis magnum)

Exempla
dotnet run -c Release -- encode input.png output.imgnum
dotnet run -c Release -- decode output.imgnum restored.png
dotnet run -c Release -- decimal output.imgnum number.txt

Aedificare
dotnet build -c Release

Forma: .imgnum

Continet:

MAGIC + versionem

ordinem pixelorum + canales

latitudinem/altitudinem

numerum totalem byte-orum ad restitutionem exactam (servat “leading zeros”)

longitudinem corporis + corpus (bytes big-endian)

Frankenstein Junkpile Transmissio (fictio scientifica)

Hoc instrumentum est fragmentum Frankenstein Junkpile, quasi artefactum e futuro in tempus nostrum delapsum.

In futuro homines mutantur: machinae carnis, machinae metalli, et postremo forma energiae, ubi identitas in schematibus et signis vivit.
Cum corpus non sit vas principale, memoria fragilis est. ImgNum unum vinculum praebet: “vitae-frame” in numerum figere, ut continuatio servetur per mutationes formarum.
