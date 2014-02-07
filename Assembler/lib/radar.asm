
; void setRadarPointer(void* ptr)
setRadarPointer:
    push r0
    mov r0, [sp + 8]
    int DEV_RADAR
    pop r0
    retn 4
