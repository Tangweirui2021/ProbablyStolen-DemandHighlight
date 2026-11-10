#!/usr/bin/env python3
"""Render a non-game schematic from masks exported by the C# integration executable.
  dotnet tests/bin/Release/net8.0/Integration.dll --preview-data /tmp/markers.json
  python3 tools/preview/render.py /tmp/markers.json preview.png
Requires Pillow only for this optional preview; it is not a mod dependency.
"""
import json
from pathlib import Path
import sys
from PIL import Image, ImageDraw, ImageFont
D=json.loads(Path(sys.argv[1]).read_text())
W,H=1000,620
im=Image.new('RGBA',(W,H),'#171720');p=ImageDraw.Draw(im)
font='/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc'
bold='/usr/share/fonts/opentype/noto/NotoSansCJK-Bold.ttc'
if not Path(font).exists():
    raise SystemExit('Set the preview font paths to locally available CJK fonts first.')
def text(x,y,s,size=18,color='#DAD7E4',heavy=False):
    p.text((x,y),s,font=ImageFont.truetype(bold if heavy else font,size),fill=color)
text(38,27,'需求框 × 永久星标',32,'#F3EAD9',True)
text(39,78,'框线看物品类型，星标看物品属性；两套标记互相独立。',18,'#A9ACBD')
p.line((38,115,962,115),fill='#3B3B50',width=1)
xs=[38,276,514,752]
def mask_layer(alpha,w,h,color,bottom_up=False):
    a=Image.new('L',(w,h));a.putdata(alpha)
    if bottom_up:a=a.transpose(Image.Transpose.FLIP_TOP_BOTTOM)
    layer=Image.new('RGBA',(w,h),color);layer.putalpha(a)
    return layer

def card(x,y,title,kind,color=None,risk=None,container=False):
    # 100x64 logical UI units, at exactly 2x for a crisp, inspectable schematic.
    tile=Image.new('RGBA',(100,64),'#292A3A');q=ImageDraw.Draw(tile)
    for xx in range(0,101,20):q.line((xx,0,xx,64),fill='#333446')
    for yy in range(0,65,16):q.line((0,yy,100,yy),fill='#333446')
    if color:
        tile.alpha_composite(mask_layer(D['container'] if container else D['frame'],100,64,color,True))
    if risk:
        side=D['side'];n=side*2
        for key,c in [('edge','#211C30'),('fill',risk)]:
            star=mask_layer(D[key],side,side,c).resize((n,n),Image.Resampling.NEAREST)
            tile.alpha_composite(star,(100-n-2,2))
    tile=tile.resize((200,128),Image.Resampling.NEAREST)
    im.alpha_composite(tile,(x,y))
    text(x+17,y+42,kind,23,'#DFD8C9',True)
    text(x,y+139,title,17,'#DFD8C9')

text(38,130,'同一种食物：框色始终相同',18,'#A9ACBD')
card(xs[0],168,'普通商品','食物','#F5B66C')
card(xs[1],168,'仅赃物 → 黄星','食物','#F5B66C','#FFD84A')
card(xs[2],168,'仅违禁品 → 绿星','食物','#F5B66C','#66ED80')
card(xs[3],168,'两种属性 → 红星','食物','#F5B66C','#FF5656')
text(38,350,'其他情况：需求框可以消失，属性星标仍保留',18,'#A9ACBD')
card(xs[0],389,'顾客需要：按工具色画框','工具','#FFDB76')
card(xs[1],389,'顾客不需要：只有星标','工具',None,'#FFD84A')
card(xs[2],389,'里面有需求商品：四角提示','容器','#80D8FF',None,True)
card(xs[3],389,'没有顾客：依然显示星标','商品',None,'#FF5656')
text(38,580,'依据代码中的像素掩码渲染的示意图 · 非游戏截图 · 所有标记颜色均可配置',15,'#858A9F')
im.convert('RGB').save(sys.argv[2])
