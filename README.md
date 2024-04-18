# Assetto Corsa FMOD Parser
Very rough WIP project to parse some data from FMOD .bank files for Assetto Corsa.

## Usage
Drag & drop a sound bank (.bank) file onto the exe. This will extract all sound samples from the sound bank and produce 
a new sound bank with stereo samples converted to mono if left/right audio channels are mostly identical.

```
Description:
  Assetto Corsa FMOD Parser

  By default this tool will extract all samples from a sound bank and write three files:
  <filename>_structure.txt: A file outlining the contents of the FMOD sound bank
  <filename>_samples.txt: A file showing some info about included sound samples
  <filename>_out.bank: The sound bank with sounds downsampled to mono according to the
  downsample-threshold parameter

Usage:
  FmodParser <input-file> [options]

Arguments:
  <input-file>  The input sound bank (*.bank)

Options:
  -d, --downsample-threshold <downsample-threshold>  Maximum deviation between left/right channels        
                                                     required for downsampling from stereo to mono        
                                                     [default: 5]
  -r, --replace                                      Instead of extracting a sound bank, replace all      
                                                     files in the sound bank with files from a folder     
                                                     with the same name
  --version                                          Show version information
  -?, -h, --help                                     Show help and usage information
```