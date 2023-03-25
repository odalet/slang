# CMakeList.txt : CMake project for slang, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.13)

# Add source to this project's executable.
add_executable (slc "main.cpp"  "list.h" "util.h" "util.cpp" "buffer.h" "buffer.cpp")
set_target_properties(slc PROPERTIES
    VS_DEBUGGER_WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    VS_DEBUGGER_COMMAND_ARGUMENTS "test.sl")

# TODO: Add tests and install targets if needed.
