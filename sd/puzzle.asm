; all numbers are octal
; some simple math, memory access, then HALT

    ORG 40
START:
    LDI 20
    LDI 20
    ADD
    STOR 0010
    HALT