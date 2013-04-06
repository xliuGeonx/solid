#!/bin/bash

# Set up the environment. Needed before building or starting.

platform='unknown'
unamestr=`uname`
if [[ "$unamestr" == 'Linux' ]]; then
   platform='linux'
elif [[ "$unamestr" == 'Darwin' ]]; then
   platform='macos'
fi

if [[ $platform == 'linux' ]]; then
    export MONO_PATH="$MONO_PATH:/usr/lib/cli/gtk-sharp-2.0/:/usr/lib/cli/gdk-sharp-2.0/:/usr/lib/cli/pango-sharp-2.0/"
elif [[ $platform == 'macos' ]]; then
    export DYLD_FALLBACK_LIBRARY_PATH="$DYLD_FALLBACK_LIBRARY_PATH:/Library/Frameworks/Mono.framework/Versions/Current/lib:/usr/lib"
    export MONO_PATH="$MONO_PATH:/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/gtk-sharp-2.0:/Library/Frameworks/Mono.framework/Versions/Current/lib"
fi
