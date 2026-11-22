# Logo üretir: logo.png (1024), logo96.png, logo.ico
import glob, os
from PIL import Image, ImageDraw, ImageFont

here = os.path.dirname(os.path.abspath(__file__))
adaylar = [r"C:\Windows\Fonts\arialbd.ttf", r"C:\Windows\Fonts\segoeuib.ttf"] + \
    glob.glob('/usr/share/fonts/**/DejaVuSans-Bold.ttf', recursive=True)
fp = next(f for f in adaylar if os.path.exists(f))

S = 1024
im = Image.new('RGBA', (S, S), (0, 0, 0, 0))
mask = Image.new('L', (S, S), 0)
ImageDraw.Draw(mask).rounded_rectangle([0, 0, S - 1, S - 1], 220, fill=255)
bg = Image.new('RGBA', (S, S))
bd = ImageDraw.Draw(bg)
bd.rectangle([0, 0, S, S], fill=(20, 24, 31, 255))
bd.polygon([(S, 0), (S, S), (0, S)], fill=(10, 92, 255, 255))
im.paste(bg, (0, 0), mask)
d = ImageDraw.Draw(im)
f = ImageFont.truetype(fp, 520)

def c(t, cx, cy):
    b = d.textbbox((0, 0), t, font=f)
    w, h = b[2] - b[0], b[3] - b[1]
    d.text((cx - w / 2 - b[0], cy - h / 2 - b[1]), t, font=f, fill=(255, 255, 255, 255))

c('$', 330, 330)
c('\u20ba', 694, 694)
im.save(os.path.join(here, 'logo.png'))
im.save(os.path.join(here, 'logo.ico'), sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])
im.resize((96, 96), Image.LANCZOS).save(os.path.join(here, 'logo96.png'))
