# YOLO
  
OpenCvSharpによるDarknetフレームワークのモデル読み込みライブラリです。  
YOLO v3系・v4系(tinyもok)が動作することは確認しています。v2はたぶん無理。v5は不明。  
  
YOLOとは？という方はコチラ↓↓  
* [【物体検出手法の歴史 : YOLOの紹介】](https://qiita.com/cv_carnavi/items/68dcda71e90321574a2b)  
* [【論文紹介】YOLOの論文を読んだので要点をまとめてみた](https://dev.classmethod.jp/articles/research_paper_yolo/)  
  
### <使用例>  
```
var detector = new Yolo(cfg, names, weights, new Size(640, 480), DrawingMode.Rectangle, 0.5f, 0.3f);  
detector.Run(ref frame, out var results);  
```  
  
出力のYoloResultsクラスはそれぞれList型のLabels, Confidences, Centers, Sizesに加え、インデクサを実装、IEnumerable<>・IEnumerator<>を継承しているのでforeachとLinqが使えます。  
これの何がありがたいかというと、  
  
```
detector.Run(ref frame, out var results);  
SomeFunction(results.Centers);  
```
↑↑↑　のように各要素のListを取り出せるほか、  
  
```  
detector.Run(ref frame, out var results);  
for (int i = 0; i < results.Count; i++)  
{  
　　// 次のコードはどちらも同じ意味  
　　SomeFunction(results.Centers[i]);  
　　SomeFunction(results[i].Center);  
}  
```  
↑↑↑　for文でのアクセスの仕方が自由になり、  
```  
detector.Run(ref frame, out var results);  
var list = new List<(string, Point, Size)>();  
foreach (var r in results)  
{  
　　list.Add((r.Label, r.Center, r.Size));  
}  
SomeFunction(list);  
```  
↑↑↑　foreach文内でも要素を取り出せたり、  
  
```  
detector.Run(ref frame, out var results);
var list = results  
　　.Select(r => (r.Label, r.Center, r.Size))  
　　.ToList();  
SomeFunction(list);  
``` 
↑↑↑　なんならLinq使えばforeachすら要らなかったり、  
  
```
detector.Run(ref frame, out var results);  
var list = results  
　　.Where(r => r.Label == "pumpkin")  
　　.Where(r => r.Confidence > 0.8)
　　.Select(r => r.Center)  
　　.ToList();  
SomeFunction(list);  
```  
↑↑↑　条件検索をかけることが可能になります。  

# SVM (Support Vector Machine)
気が向いたらちゃんと説明書きます。とりあえずこれでも見といてください。↓↓  
[SVM(サポートベクターマシーン)についてまとめてみた](https://qiita.com/arata-honda/items/bc24cbd953bd9d2c743c)
