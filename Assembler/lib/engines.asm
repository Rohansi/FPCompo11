
; void setThrustSpeed(int speed)
setThrustSpeed:
    push r7
    push r8

    xor r7, r7
    mov r8, [sp + 8]
    int DEV_ENGINES

    pop r8
    pop r7
    ret

; void setTurnSpeed(int speed)
setTurnSpeed:
    push r7
    push r8

    mov r7, 1
    mov r8, [sp + 8]
    int DEV_ENGINES

    pop r8
    pop r7
    ret
