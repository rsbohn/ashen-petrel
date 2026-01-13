;; Print asciiz string
ORG 10
PUTS:
    ZERO, STAX    ; string index
    LOAD MESSAGE,X
    BE P+4
    WIO 0
    INCX
    BR P-4
    HALT

ORG 60
MESSAGE:
    DW #104 #117 #109 #105 #108 #105 #116 #121 
    DW 014 0
