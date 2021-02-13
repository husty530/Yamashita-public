using System.Collections.Generic;
using OpenCvSharp;

namespace Yamashita.MultiTracker
{
    public interface IMultiTracker
    {
        /// <summary>
        /// 観測を使って追跡状態を更新する
        /// </summary>
        /// <param name="frame">入出力画像</param>
        /// <param name="detections">検出結果のリスト</param>
        /// <param name="results">更新結果</param>
        public void Update(
            ref Mat frame,
            List<(string Label, Point Center, Size Size)> detections,
            out List<(int Id, string Label, float Iou, Point Center, Size Size)> results
            );
    }
}
