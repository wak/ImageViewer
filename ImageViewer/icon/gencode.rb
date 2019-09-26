#! /usr/bin/env ruby

require 'base64'

$output = ''
$output += <<-STR
using System;

namespace ImageViewer
{
    class FormIcon
    {
STR

def emitBase64(basename, filepath)
  $output += <<-STR
        static string ICON_BASE64_#{basename.upcase} = @"
#{Base64.encode64(File.read(filepath)).rstrip}";

  STR
end

def emitCode(basename)
  $output += <<-STR
        static public System.Drawing.Icon #{basename}Icon()
        {
            byte[] byteArray = Convert.FromBase64String(ICON_BASE64_#{basename.upcase});
            return new System.Drawing.Icon(new System.IO.MemoryStream(byteArray));
        }

  STR
end

def emitGetIcon(files)
  $output += <<-STR
        static public System.Drawing.Icon getIcon(string name)
        {
            switch (name)
            {
  STR

  files.each {|file|
    basename = File.basename(file).slice(/(.*)\.ico/, 1)
    $output += <<-STR
                case "#{basename}":
                    return #{basename}Icon();
    STR
  }
  
  $output += <<-STR
                default:
                    throw new Exception("unknown icon");
            }
        }
  STR
end

mydir = File.dirname(File.expand_path(__FILE__))
Dir.chdir(mydir)
files = Dir.glob("*.ico").map {|f| File.join(mydir, f) }

files.each {|file|
  basename = File.basename(file).slice(/(.*)\.ico/, 1)
  emitBase64(basename, file)
}

files.each {|file|
  basename = File.basename(file).slice(/(.*)\.ico/, 1)
  emitCode(basename)
}

emitGetIcon(files)

$output += <<-STR
    }
}
STR


puts $output.gsub("\n", "\r\n")
