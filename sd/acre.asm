    ORG 40
START:
    LDXI 100
    LDI 377
    DUP
    STOR DB+077,X
    DXBZ .+2
    BR .-3
    HALT
