/*
 * Copyright (c) 2015 Andrew Kelley
 *
 * This file is part of zig, which is MIT licensed.
 * See http://opensource.org/licenses/MIT
 */

#ifndef SL_LIST_H
#define SL_LIST_H

#include "util.h"

#include <assert.h>
#include <limits.h>

template<typename T>
struct ZigList {
    void deinit() {
        free(items);
    }
    void append(T item) {
        ensure_capacity(length + 1);
        items[length++] = item;
    }
    // remember that the pointer to this item is invalid after you
    // modify the length of the list
    const T& at(size_t index) const {
        assert(index != SIZE_MAX);
        assert(index < length);
        return items[index];
    }
    T& at(size_t index) {
        assert(index != SIZE_MAX);
        assert(index < length);
        return items[index];
    }
    T pop() {
        assert(length >= 1);
        return items[--length];
    }

    T* add_one() {
        resize(length + 1);
        return &last();
    }

    const T& last() const {
        assert(length >= 1);
        return items[length - 1];
    }

    T& last() {
        assert(length >= 1);
        return items[length - 1];
    }

    void resize(size_t new_length) {
        assert(new_length != SIZE_MAX);
        ensure_capacity(new_length);
        length = new_length;
    }

    void clear() {
        length = 0;
    }

    void ensure_capacity(size_t new_capacity) {
        if (capacity >= new_capacity)
            return;

        size_t better_capacity = capacity;
        do {
            better_capacity = better_capacity * 5 / 2 + 8;
        } while (better_capacity < new_capacity);

        items = reallocate_nonzero(items, capacity, better_capacity);
        capacity = better_capacity;
    }

    T* items;
    size_t length;
    size_t capacity;
};

#endif
