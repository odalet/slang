#ifndef SL_UTIL_H
#define SL_UTIL_H

#if defined(_MSC_VER)

#include <intrin.h>

#define ATTRIBUTE_COLD __declspec(noinline)
#define ATTRIBUTE_PRINTF(a, b)
#define ATTRIBUTE_RETURNS_NOALIAS __declspec(restrict)
#define ATTRIBUTE_NORETURN __declspec(noreturn)

#else

#define ATTRIBUTE_COLD         __attribute__((cold))
#define ATTRIBUTE_PRINTF(a, b) __attribute__((format(printf, a, b)))
#define ATTRIBUTE_RETURNS_NOALIAS __attribute__((__malloc__))
#define ATTRIBUTE_NORETURN __attribute__((noreturn))

#endif

ATTRIBUTE_COLD
ATTRIBUTE_NORETURN
ATTRIBUTE_PRINTF(1, 2)
void slang_panic(const char* format, ...);

template<typename T>
static inline void safe_memcpy(T* dest, const T* src, size_t count) {
#ifdef NDEBUG
    memcpy(dest, src, count * sizeof(T));
#else
    // manually assign every elment to trigger compile error for non-copyable structs
    for (size_t i = 0; i < count; i += 1) {
        dest[i] = src[i];
    }
#endif
}

template<typename T>
ATTRIBUTE_RETURNS_NOALIAS static inline T* allocate_nonzero(size_t count) {
    T* ptr = reinterpret_cast<T*>(malloc(count * sizeof(T)));
    if (!ptr)
        slang_panic("allocation failed");
    return ptr;
}

template<typename T>
ATTRIBUTE_RETURNS_NOALIAS static inline T* allocate(size_t count) {
    T* ptr = reinterpret_cast<T*>(calloc(count, sizeof(T)));
    if (!ptr)
        slang_panic("allocation failed");
    return ptr;
}

template<typename T>
static inline T* reallocate_nonzero(T* old, size_t old_count, size_t new_count) {
#ifdef NDEBUG
    T* ptr = reinterpret_cast<T*>(realloc(old, new_count * sizeof(T)));
    if (!ptr)
        zig_panic("allocation failed");
    return ptr;
#else
    // manually assign every element to trigger compile error for non-copyable structs
    T* ptr = allocate_nonzero<T>(new_count);
    safe_memcpy(ptr, old, old_count);
    free(old);
    return ptr;
#endif
}

#endif