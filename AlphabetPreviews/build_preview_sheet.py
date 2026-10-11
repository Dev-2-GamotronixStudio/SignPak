from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).parent
font_path = 'C:/Windows/Fonts/arial.ttf'
title_font = ImageFont.truetype(font_path, 32)
label_font = ImageFont.truetype(font_path, 24)
small_font = ImageFont.truetype(font_path, 18)
sheet = Image.new('RGB', (1500, 1580), '#171c24')
draw = ImageDraw.Draw(sheet)
draw.text((24, 20), 'SignPAK | English alphabet animation drafts', font=title_font, fill='white')
draw.text((24, 65), 'Humanoid clips · 3 seconds · Jack rig · J and Z include movement', font=small_font, fill='#b7c4d4')
for i, letter in enumerate('ABCDEFGHIJKLMNOPQRSTUVWXYZ'):
    x, y = 20 + (i % 6) * 246, 105 + (i // 6) * 282
    image = Image.open(root / f'{letter}-hand.png').resize((226, 226))
    sheet.paste(image, (x, y + 32))
    draw.text((x + 8, y), letter, font=label_font, fill='white')
draw.text((24, 1535), 'Review thumb contacts in K/M/N/T and crossing in R with your PSL reference before teaching.', font=small_font, fill='#b7c4d4')
sheet.save(root / 'Alphabet-review.png')
for letter in ['J', 'Z']:
    strip = Image.new('RGB', (1200, 350), '#171c24')
    d = ImageDraw.Draw(strip)
    for i, state in enumerate(['start', 'stroke1', 'stroke2', 'finish']):
        im = Image.open(root / f'{letter}-{state}.png').resize((300, 300))
        strip.paste(im, (300*i, 40))
        d.text((300*i + 15, 8), f'{letter}: {state}', font=label_font, fill='white')
    strip.save(root / f'{letter}-motion.png')
