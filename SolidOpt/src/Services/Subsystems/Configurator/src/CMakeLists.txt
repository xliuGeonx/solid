##
## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

csharp_add_library(
  SolidOpt.Services.Subsystems.Configurator.dll
  ${CMAKE_CURRENT_SOURCE_DIR}/*.cs
  ${CMAKE_CURRENT_SOURCE_DIR}/Mappers/*.cs
  ${CMAKE_CURRENT_SOURCE_DIR}/Sources/*.cs
  ${CMAKE_CURRENT_SOURCE_DIR}/Targets/*.cs
  ${CMAKE_CURRENT_SOURCE_DIR}/TypeResolvers/*.cs
  ${CMAKE_CURRENT_BINARY_DIR}/AssemblyInfo.cs
  )
csharp_add_dependencies(
  SolidOpt.Services.Subsystems.Configurator.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Subsystems.Cache.dll
  ${CMAKE_LIBRARY_OUTPUT_DIR}/SolidOpt.Services.Subsystems.HetAccess.dll
  # External
  ${CMAKE_LIBRARY_OUTPUT_DIR}/Mono.Cecil.dll
  )
