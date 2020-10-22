//
//  LUImageUtils.m
//
//  Lunar Unity Mobile Console
//  https://github.com/SpaceMadness/lunar-unity-console
//
//  Copyright 2015-2020 Alex Lementuev, SpaceMadness.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//


#import "LUImageUtils.h"
#import "Lunar.h"

UIImage *LUGetImage(NSString *name)
{
    UIImage *image = [UIImage imageNamed:name];
    LUAssertMsgv(image != nil, @"Can't load image: %@", name);
    return image;
}

UIImage *LUGet3SlicedImage(NSString *name)
{
    UIImage *image = LUGetImage(name);
    LUAssertMsgv(((int)image.size.width) % 3 == 0, @"3 sliced image has wrong width: %g", image.size.width);
    return [image stretchableImageWithLeftCapWidth:image.size.width / 3 topCapHeight:0];
}

UIColor * LUUIColorFromRGB(NSUInteger value) {
    static const float multipler = 1.0 / 255.0;
    return [UIColor colorWithRed:((float)((value & 0xFF000000) >> 24)) * multipler
                           green:((float)((value & 0xFF0000) >> 16)) * multipler
                            blue:((float)((value & 0xFF00) >> 8)) * multipler
                           alpha:((float)(value & 0xFF)) * multipler];
}
