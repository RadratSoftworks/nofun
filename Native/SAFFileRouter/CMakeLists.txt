project(SAFFileRouter)

cmake_minimum_required(VERSION 3.1)
set(CMAKE_CXX_STANDARD 17)

add_library(SAFFileRouter SHARED router.cpp)