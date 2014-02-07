
; void setShooting(bool shooting)
setShooting:
    push r0
    mov r0, [sp + 8]
    int DEV_GUNS
    pop r0
    retn 4
