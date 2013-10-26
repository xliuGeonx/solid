#
# A CMake Module for using MSBuild.
#
# The following macros are set:
#   CSHARP_ADD_MSBUILD_PROJECT
#
# Copyright (c) SolidOpt Team
#

function( CSHARP_ADD_MSBUILD_PROJECT filename )
  # TODO: Check if it was executable and set it properly
  set( TYPE_UPCASE "LIBRARY" )
  set( output "dll" )

  get_filename_component(name ${filename} NAME)
  STRING( REGEX REPLACE "(\\.csproj)[^\\.csproj]*$" "" name_we ${name} )
  STRING( REGEX REPLACE "(\\.sln)[^\\.sln]*$" "" name_we ${name_we} )

  if ( "${name}" MATCHES "^.*\\.dll$" )
    MESSAGE(FATAL "Do not use CSHARP_ADD_MSBUILD_PROJECT with dlls. For dlls use CSHARP_ADD_LIBRARY_BINARY instead.")
  endif()

  # Add custom target and command
  MESSAGE( STATUS "Adding project ${filename} for MSBuild-ing." )

  # Copy and adapt the file to the CMAKE setup
  file (READ "${filename}" CSPROJ_FILE)
  # We need to replace some the csproj file with the currently active cmake
  # configuration. Thus it is better to copy the file in our build folder and
  # make the substitutions there. 
  # Settings that should be changed are:
  #   - OutputPath
  #   - DocumentationFile
  #   - TargetFrameworkVersion
  #   - DebugType
  #   - <Compile Include="AssemblyInfo.cs" ...
  #   - <EmbeddedResource Include=" ...
  #   - <Content Include="..
  #   - <None Include="..." ...
  #   - <AssemblyOriginatorKeyFile>...
  string(REGEX REPLACE 
    "<OutputPath>.*</OutputPath>" 
    "<OutputPath>${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}</OutputPath>"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  # FIXME: Extract the file component from the path and put it as postfix 
  string(REGEX REPLACE 
    "<DocumentationFile>^[\\r\\n\\t ]+</DocumentationFile>" 
    "<DocumentationFile>${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}</DocumentationFile>"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REGEX REPLACE
    "<TargetFrameworkVersion>.*</TargetFrameworkVersion>"
    "<TargetFrameworkVersion>v${CSHARP_FRAMEWORK_VERSION}</TargetFrameworkVersion>"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  get_filename_component(filename_path "${filename}" PATH)
  file(RELATIVE_PATH rel_filename_path ${CMAKE_SOURCE_DIR} ${filename_path})
  #set(msbuild_path "${CMAKE_SOURCE_DIR}/${rel_filename_path}/")
  string(REPLACE "/" "\\" msbuild_path "${CMAKE_SOURCE_DIR}/${rel_filename_path}/")
  #string(REGEX REPLACE "(.*<DebugType>{)(.*)(}</DebugType>.*)" "\\2" ${CMAKE_BUILD_TYPE} "${CSPROJ_FILE}")
  string(REPLACE
    "<Compile Include=\""
    "<Compile Include=\"${msbuild_path}"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<EmbeddedResource Include=\""
    "<EmbeddedResource Include=\"${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<Content Include=\""
    "<Content Include=\"${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<None Include=\""
    "<None Include=\"${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<AssemblyOriginatorKeyFile>"
    "<AssemblyOriginatorKeyFile>${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REGEX REPLACE 
    "(.*<Project.*ToolsVersion=\")([0-9]\\.[0-9])(\".*>)" 
    "\\1${CSHARP_FRAMEWORK_VERSION}\\3"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )

  # We need to extract:
  #   - output_type
  #   - proj_guid
  #   - type
  string(REGEX REPLACE 
    "(.*<ProjectGuid>{)(.*)(}</ProjectGuid>.*)" "\\2" proj_guid "${CSPROJ_FILE}"
    )
  string(REGEX REPLACE
    "(.*<OutputType>)(.*)(</OutputType>.*)" "\\2" output_type "${CSPROJ_FILE}"
    )
  set(type "${output_type}")
  string(TOLOWER output_type "${output_type}")

  # Save the new csproj file
  # Create the missing directories
  file(RELATIVE_PATH new_csproj_filename ${CMAKE_SOURCE_DIR} ${filename})
  set(new_csproj_filename "${CMAKE_BINARY_DIR}/${new_csproj_filename}")
  file(WRITE "${new_csproj_filename}" "${CSPROJ_FILE}")

  # Save project info in global properties
  # Put the dll instead of ${name} because the name ends with csproj and the
  # dependency checker expects to be an output type.
  set_property(GLOBAL APPEND PROPERTY target_name_property "${name_we}.${output}")
  set_property(GLOBAL APPEND PROPERTY target_type_property "${type}")
  set_property(GLOBAL APPEND PROPERTY target_output_type_property "${output_type}")
  set_property(GLOBAL APPEND PROPERTY target_out_property "${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name}")
  set_property(GLOBAL APPEND PROPERTY target_guid_property "${proj_guid}")
  # The implementation relies on fixed numbering. I.e. every property should be
  # set for each target in order CMAKE and VS sln generation to work. In the
  # case of references and test cases we have to insert sort-of empty property
  # for each target. In the case where the current target has test cases or more
  # references we have to edit the string at that position.
  set_property(GLOBAL APPEND PROPERTY target_refs_property "#")
  set_property(GLOBAL APPEND PROPERTY target_metas_key_property "#")
  set_property(GLOBAL APPEND PROPERTY target_metas_value_property "#")
  set_property(GLOBAL APPEND PROPERTY target_tests_property "#")
  set_property(GLOBAL APPEND PROPERTY target_test_results_property "#")
  # No need of sources
  set_property(GLOBAL APPEND PROPERTY target_sources_property "#")
  # No need of source dependencies
  set_property(GLOBAL APPEND PROPERTY target_sources_dep_property "#")
  set_property(GLOBAL APPEND PROPERTY target_src_dir_property "${CMAKE_CURRENT_SOURCE_DIR}")
  set_property(GLOBAL APPEND PROPERTY target_bin_dir_property "${CMAKE_CURRENT_BINARY_DIR}")
  # Empty will signal that the csproj is already built
  set_property(GLOBAL APPEND PROPERTY target_proj_file_property "${new_csproj_filename}") 
  set_property(GLOBAL APPEND PROPERTY target_generate_proj_file_property FALSE)
  #set_target_properties("${name_we}.${output}" PROPERTIES csproj "${new_csproj_filename}")


  #
  #list(APPEND MSBUILDFLAGS "/p:OutputPath=${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}")
  add_custom_command(
    COMMENT "MSBuilding: ${MSBUILD} ${MSBUILDFLAGS} ${new_csproj_filename}"
    OUTPUT ${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name_we}.${output}
    COMMAND ${MSBUILD}
    ARGS ${MSBUILDFLAGS} ${new_csproj_filename}
    WORKING_DIRECTORY ${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}
    )
  add_custom_target(
    "${name_we}.${output}" ALL
    DEPENDS ${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name_we}.${output}
    SOURCES ${new_csproj_filename}
    )

endfunction( CSHARP_ADD_MSBUILD_PROJECT)
