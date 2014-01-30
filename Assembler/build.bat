@echo off
if not defined include set include=include;.
fasm enemy.asm ../GlitchGame/Data/enemy.bin
fasm drone.asm ../GlitchGame/Data/drone.bin
