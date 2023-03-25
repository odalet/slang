#include <stdlib.h>
#include <stdio.h>
#include <stdarg.h>
#include <cstdint>

#include "util.h"

void slang_panic(const char* format, ...) {
    va_list ap;
    va_start(ap, format);
    vfprintf(stderr, format, ap);
    fprintf(stderr, "\n");
    fflush(stderr);
    va_end(ap);
    abort();
}

uint32_t int_hash(int i) {
    return (uint32_t)(i % UINT32_MAX);
}
bool int_eq(int a, int b) {
    return a == b;
}

uint32_t uint64_hash(uint64_t i) {
    return (uint32_t)(i % UINT32_MAX);
}

bool uint64_eq(uint64_t a, uint64_t b) {
    return a == b;
}

uint32_t ptr_hash(const void* ptr) {
    return (uint32_t)(((uintptr_t)ptr) % UINT32_MAX);
}

bool ptr_eq(const void* a, const void* b) {
    return a == b;
}
