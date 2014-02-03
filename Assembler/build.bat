@echo off
if not defined include set include=include;.

set outdir=../GlitchGame/Data

fasm enemy.asm  %outdir%/enemy.bin -s %outdir%/enemy.fas
fasconvert      %outdir%/enemy.fas

fasm drone.asm  %outdir%/drone.bin -s %outdir%/drone.fas
fasconvert      %outdir%/drone.fas
