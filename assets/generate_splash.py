from PIL import Image

BACKGROUND_COLOR = (21, 128, 61)
CANVAS_SIZE = (1284, 2778)
LOGO_SIZE = (800, 800)

canvas = Image.new('RGB', CANVAS_SIZE, BACKGROUND_COLOR)
logo = Image.open('assets/icon.png').convert('RGBA')
logo = logo.resize(LOGO_SIZE, Image.LANCZOS)

x = (CANVAS_SIZE[0] - LOGO_SIZE[0]) // 2
y = (CANVAS_SIZE[1] - LOGO_SIZE[1]) // 2

canvas.paste(logo, (x, y), logo)
canvas.save('assets/splash.png')
print("splash.png yaratildi!")