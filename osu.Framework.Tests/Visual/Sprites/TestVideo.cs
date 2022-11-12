// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.IO;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Video;

namespace osu.Framework.Tests.Visual.Sprites
{
    internal class TestVideo : Video
    {
        public TestVideo(Stream videoStream, bool startAtCurrentTime = true)
            : base(videoStream, startAtCurrentTime)
        {
        }

        private bool? useRoundedShader;

        public bool? UseRoundedShader
        {
            get => useRoundedShader;
            set
            {
                useRoundedShader = value;

                if (value == true)
                {
                    Masking = true;
                    CornerRadius = 10f;
                }
                else
                {
                    Masking = false;
                    CornerRadius = 0f;
                }

                Invalidate(Invalidation.DrawNode);
            }
        }

        public override Drawable CreateContent() => Sprite = new VideoSprite(this) { RelativeSizeAxes = Axes.Both };
    }
}
