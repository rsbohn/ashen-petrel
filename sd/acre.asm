    ORG 40
START:
    LDXI 100
    LDI 377
    DUP
    STOR DB+077,X
    DXBZ P+2
    BR P-3
    HALT
