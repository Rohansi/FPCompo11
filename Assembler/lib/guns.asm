
; void setShooting(bool shooting)
setShooting:
    push r7

    mov r7, [sp + 8]
    int DEV_GUNS

    pop r7
    ret
