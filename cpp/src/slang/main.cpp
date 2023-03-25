// This is (more than) heavily inspired by zig 0.1.1 source code! See https://ziglang.org/download/
#include <cstdio>
#include <filesystem>

#include "list.h"
#include "buffer.h"

int main(int argc, char** argv) {

    fprintf(stdout, "CWD: %ls\n", std::filesystem::current_path().c_str());

    const char* in_file = nullptr;
    const char* out_file = nullptr;

#if _DEBUG
    in_file = "../../../../test.sl";
#else
    if (argc < 2) {
        fprintf(stderr, "Expected command argument\n");
        return 1;
    }

    in_file = argv[1];
#endif

    auto in_file_buf = buf_create_from_str(in_file);
    Buf* zig_root_source_file = in_file_buf;

    // TODO
    //CodeGen* g = codegen_create(zig_root_source_file, target, out_type, build_mode, zig_lib_dir_buf); 

    fprintf(stdout, "Building %s", in_file);
    return 0;
}
