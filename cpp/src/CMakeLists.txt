# CMakeList.txt : Top-level CMake project file, do global configuration
# and include sub-projects here.
#
cmake_minimum_required (VERSION 3.13)

#if(MSVC)
#set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /std:c++17")
#else()
#set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++17")
#endif(MSVC)

project (slang)

# Include sub-projects.
add_subdirectory ("slang")

target_compile_features(slc PRIVATE cxx_std_20)