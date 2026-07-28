cmake_minimum_required(VERSION 3.20)
project(lbg_native VERSION 0.1.0 LANGUAGES C)

set(LIBREDWG_SOURCE_DIR "" CACHE PATH "LibreDWG source directory")
set(LIBREDWG_BINARY_DIR "" CACHE PATH "LibreDWG build directory")

if(NOT EXISTS "${LIBREDWG_SOURCE_DIR}/include/dwg.h")
  message(FATAL_ERROR "Set LIBREDWG_SOURCE_DIR to a LibreDWG 0.13.4 checkout")
endif()
if(NOT EXISTS "${LIBREDWG_BINARY_DIR}/Release/libredwg.lib")
  message(FATAL_ERROR "Set LIBREDWG_BINARY_DIR to a built LibreDWG directory")
endif()

add_library(lbg_native SHARED src/lbg_native.c)
target_compile_features(lbg_native PRIVATE c_std_11)
target_compile_definitions(lbg_native PRIVATE LBG_NATIVE_EXPORTS)
if(MSVC)
  target_compile_options(lbg_native PRIVATE /wd4819)
endif()
target_include_directories(lbg_native PRIVATE
  "${LIBREDWG_SOURCE_DIR}/include"
  "${LIBREDWG_BINARY_DIR}/src")
target_link_libraries(lbg_native PRIVATE
  "${LIBREDWG_BINARY_DIR}/Release/libredwg.lib")

add_custom_command(TARGET lbg_native POST_BUILD
  COMMAND ${CMAKE_COMMAND} -E copy_if_different
    "${LIBREDWG_BINARY_DIR}/Release/libredwg.dll"
    "$<TARGET_FILE_DIR:lbg_native>/libredwg.dll")
