;; Print asciiz string
ORG 10
PUTS:
    ZERO, STAX    ; string index
    LOAD MESSAGE,X
    BE .+4
    WIO 0
    INCX
    BR .-4
    HALT

ORG 60
MESSAGE:
    DW #104 #117 #109 #105 #108 #105 #116 #121 
    DW 014 0
