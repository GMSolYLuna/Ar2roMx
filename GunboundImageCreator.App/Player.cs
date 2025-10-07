using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GunboundTools.Archive;
using GunboundTools.Imaging;

namespace GunboundImageCreator.App
{
    public class Player
    {
        public GunboundImageFile HeadFile { get; set; }
        public GunboundAnimationFile HeadAnimation { get; private set; }

        public GunboundImageFile BodyFile { get; set; }
        public GunboundAnimationFile BodyAnimation { get; private set; }

        public GunboundImageFile FlagFile { get; set; }
        public GunboundAnimationFile FlagAnimation { get; private set; }

        public GunboundImageFile GlassesFile { get; set; }
        public GunboundAnimationFile GlassesAnimation { get; private set; }

        public GunboundImageFile AvatarFxFile { get; set; }
        public GunboundAnimationFile AvatarFxAnimation { get; set; }

        public Player(GunboundImageFile headFile, GunboundImageFile bodyFile)
        {
            HeadFile = headFile;
            BodyFile = bodyFile;
            HeadAnimation = new GunboundAnimationFile();
            BodyAnimation = new GunboundAnimationFile();
            FlagAnimation = new GunboundAnimationFile();
            GlassesAnimation = new GunboundAnimationFile();
        }

        public void LoadImages()
        {
            HeadFile.Images.Clear();
            HeadFile.LoadImages();

            BodyFile.Images.Clear();
            BodyFile.LoadImages();

            if (FlagFile != null)
            {
                FlagFile.Images.Clear();
                FlagFile.LoadImages();
            }

            if (GlassesFile != null)
            {
                GlassesFile.Images.Clear();
                GlassesFile.LoadImages();
            }

            ConstructHead();
            ConstructBody();
            ConstructFlag();
            ConstructGlasses();
        }

        private void ConstructHead()
        {
            if (HeadFile == null)
                return;

            HeadAnimation = new GunboundAnimationFile();
            var frameDuration = 2;
            var aniTimeLine = new AnimationTimeline("ani");

            for (var i = 0; i < (HeadFile.Images.Count / 2) - 1; i++)
            {
                aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
            }

            for (var i = (HeadFile.Images.Count / 2) - 1; i > 0; i--)
            {
                aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
            }

            for (var i = (HeadFile.Images.Count / 2); i < HeadFile.Images.Count; i++)
            {
                aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
            }

            for (var i = HeadFile.Images.Count - 1; i > (HeadFile.Images.Count / 2); i--)
            {
                aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
            }

            HeadAnimation.AddTimeLine(aniTimeLine);
        }

        private void ConstructBody()
        {
            if (BodyFile == null)
                return;

            BodyAnimation = new GunboundAnimationFile();
            var frameDuration = 2;
            var aniTimeLine = new AnimationTimeline("ani");

            if (BodyFile.Images.Count > 11)
            {
                frameDuration = 1;
                for (var i = 0; i < BodyFile.Images.Count - 1; i++)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                BodyAnimation.AddTimeLine(aniTimeLine);
            }
            else
            {
                frameDuration = 1;
                for (var i = 0; i < BodyFile.Images.Count - 1; i++)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                for (var i = BodyFile.Images.Count - 1; i > 0; i--)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                BodyAnimation.AddTimeLine(aniTimeLine);
            }
        }


        private void ConstructGlasses()
        {
            if (GlassesFile == null)
                return;

            GlassesAnimation = new GunboundAnimationFile();
            int frameDuration;
            var aniTimeLine = new AnimationTimeline("ani");

            if (GlassesFile.Images.Count > 11)
            {
                frameDuration = 1;

                if (GlassesFile.Images.Count > 22)
                {
                    for (var i = 0; i < GlassesFile.Images.Count - 1; i++)
                    {
                        aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                    }
                }
                else
                {
                    for (var i = 0; i < (GlassesFile.Images.Count / 2) - 1; i++)
                    {
                        aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                    }

                    for (var i = (GlassesFile.Images.Count / 2) - 1; i > 0; i--)
                    {
                        aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                    }

                    for (var i = (GlassesFile.Images.Count / 2); i < GlassesFile.Images.Count; i++)
                    {
                        aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                    }

                    for (var i = GlassesFile.Images.Count - 1; i > (GlassesFile.Images.Count / 2); i--)
                    {
                        aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                    }
                }

                GlassesAnimation.AddTimeLine(aniTimeLine);
            }
            else
            {
                frameDuration = 1;

                for (var i = 0; i < GlassesFile.Images.Count - 1; i++)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                for (var i = GlassesFile.Images.Count - 1; i > 0; i--)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                GlassesAnimation.AddTimeLine(aniTimeLine);
            }
        }

        private void ConstructFlag()
        {
            if (FlagFile == null)
                return;

            FlagAnimation = new GunboundAnimationFile();
            int frameDuration;
            var aniTimeLine = new AnimationTimeline("ani");

            if (FlagFile.Images.Count > 11)
            {
                frameDuration = 1;
                for (var i = 0; i < FlagFile.Images.Count - 1; i++)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                FlagAnimation.AddTimeLine(aniTimeLine);
            }
            else
            {
                frameDuration = 1;
                for (var i = 0; i < FlagFile.Images.Count - 1; i++)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                for (var i = FlagFile.Images.Count - 1; i > 0; i--)
                {
                    aniTimeLine.AddFrame(new AnimationFrame {Duration = frameDuration, KeyFrame = i});
                }

                FlagAnimation.AddTimeLine(aniTimeLine);
            }
        }

        public GunboundImg GetImage(int index, GunboundImageFile imgFile)
        {
            if (imgFile.Images.Count < 1)
                return null;

            return index > imgFile.Images.Count - 1 ? imgFile.Images[0] : imgFile.Images[index];
        }
    }
}
