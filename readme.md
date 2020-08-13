# Plait

Creates a new image using swapped and/or modified ARGB values.

Useful for swapping and/or merging the channels found in texture maps.

## Usage

```
PLAIT.EXE image-files... --alpha channelId --red channelId --blue channelId --green channelId --output output-filename

image-files     One or more image files in a format supported by the C# System.Drawing.Bitmap component.

channel-id = byte-value (0-255)
            || <channel-code><image-file-index> (<a|r|g|b> <index>)
```

## `image-files`

Images may be in different formats BUT must all be of the same width and height.

Image formats supported, are those supported by the [C# System.Drawing.Bitmap](https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap?view=dotnet-plat-ext-3.1) class.

## `channel-id`

`channel-id` can be used to either overwrite the channel with a supplied byte value or by referencing the channel value from another image usign a single character channel `channel-code` and an image file index. 

Valid `channelId`'s are `a, r, g, b` representing the Alpha, Red, Green and Blue values of a given pixel.

i.e. `r2` would use the `red` channel from the second `image-file` supplied

`image-file-index` indexes are one-based. 

## `output-filename`

If not supplied, `output-filename` will default to the filename of the first image supplied with `-plaited` appended. i.e. if the first file is named `image.png` the output file will be named `image-plaited.png`.
