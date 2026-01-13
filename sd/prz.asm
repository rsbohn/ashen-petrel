;; Print asciiz string
    ORG 10
START:
    LDI PUTS
    SCAL 0
    HALT
;; or:
;; > dep 077777 13
;; > asm SCAL 1 11

PUTS:
    ZERO, STAX    ; string index
    LOAD MESSAGE,X
    BE .+4
    WIO 0
    INCX
    BR .-4
    DEL
    SXIT 0

    ORG 60
MESSAGE:
    TXT /humility / 
    DW 014 0
