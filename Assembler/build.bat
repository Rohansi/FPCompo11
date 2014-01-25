@echo off
if not defined include set include=include;.
fasm test.asm ../GlitchGame/Data/bios.bin
