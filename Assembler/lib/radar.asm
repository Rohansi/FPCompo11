
; void setRadarPointer(void* ptr)
setRadarPointer:
    push r7

    mov r7, [sp + 8]
    int DEV_RADAR

    pop r7
    ret
