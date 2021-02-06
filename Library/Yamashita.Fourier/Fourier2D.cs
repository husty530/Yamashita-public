﻿using System;
using OpenCvSharp;

namespace Yamashita.Fourier
{
    public class Fourier2D : IFourier
    {

        private readonly Mat _complex;
        private Mat _viewImg;
        private int cx;
        private int cy;
        private Rect tl;
        private Rect tr;
        private Rect bl;
        private Rect br;

        public Mat ViewImage => _viewImg;

        /// <summary>
        /// 画像データに対する2次元フーリエ変換
        /// </summary>
        /// <param name="image">入力画像</param>
        public Fourier2D(Mat image)
        {
            if (image.Type() != MatType.CV_8U) throw new Exception("MatType must be CV_8U.");
            var padded = new Mat();
            var r = Cv2.GetOptimalDFTSize(image.Rows);
            var c = Cv2.GetOptimalDFTSize(image.Cols);
            Cv2.CopyMakeBorder(image, padded, 0, r - image.Rows, 0, c - image.Cols, BorderTypes.Constant, new Scalar(0));
            padded.ConvertTo(padded, MatType.CV_32F);
            var planes = new Mat[] { padded, new Mat(padded.Rows, padded.Cols, MatType.CV_32F, new Scalar(0)) };
            _viewImg = new Mat(padded.Rows, padded.Cols, MatType.CV_8U, new Scalar(0));
            _complex = new Mat();
            Cv2.Merge(planes, _complex);
        }

        /// <summary>
        /// 離散フーリエ変換
        /// </summary>
        /// <param name="feature">画像の周波数特徴量</param>
        unsafe public void Dft(out float[] feature)
        {
            Cv2.Dft(_complex, _complex);
            Cv2.Split(_complex, out var planes);
            Cv2.Magnitude(planes[0], planes[1], planes[0]);
            var tmp = planes[0];
            feature = new float[tmp.Rows * tmp.Cols];
            var data = (float*)tmp.Data;
            for (int i = 0; i < feature.Length; i++)
                feature[i] = data[i];
            tmp += new Scalar(1);
            Cv2.Log(tmp, tmp);
            Cv2.Normalize(tmp, tmp, 255, 0, NormTypes.MinMax);
            tmp.ConvertTo(tmp, MatType.CV_8U);
            cx = tmp.Width / 2;
            cy = tmp.Height / 2;
            tl = new Rect(0, 0, cx, cy);
            tr = new Rect(cx, 0, cx, cy);
            bl = new Rect(0, cy, cx, cy);
            br = new Rect(cx, cy, cx, cy);
            var tl1 = tmp[tl];
            var tr1 = tmp[tr];
            var bl1 = tmp[bl];
            var br1 = tmp[br];
            _viewImg = new Mat(tmp.Rows, tmp.Cols, MatType.CV_8U);
            tl1.CopyTo(_viewImg[br]);
            tr1.CopyTo(_viewImg[bl]);
            bl1.CopyTo(_viewImg[tr]);
            br1.CopyTo(_viewImg[tl]);
        }

        /// <summary>
        /// 逆変換
        /// </summary>
        /// <param name="feature"></param>
        unsafe public void Idft(out float[] feature)
        {
            Cv2.Idft(_complex, _complex, DftFlags.Scale);
            Cv2.Split(_complex, out Mat[] planes);
            var tmp = planes[0][new Rect(0, 0, planes[0].Width, planes[0].Height)].Clone();
            feature = new float[tmp.Rows * tmp.Cols];
            var data = (float*)tmp.Data;
            for (int i = 0; i < feature.Length; i++)
                feature[i] = data[i];
            Cv2.Normalize(tmp, tmp, 255, 0, NormTypes.MinMax);
            tmp.ConvertTo(_viewImg, MatType.CV_8U);
        }

    }
}