# Tzukizi-Search

[東京都中央卸売市場日報](http://www.shijou-nippo.metro.tokyo.jp/) で公開されているデータを検索するツールです。日報データは、一度 Azure cosmos DB に吸い上げて検索するようにしています。

現在のところ過去5年間のデータ（2013年1月から2018年10月末まで）があります。

## TukiziSearch 

条件を設定して検索ができます。グリッドは Ctrl+C でコピーができるので、Excel に貼り付けてグラフ化できます。

![](images/sample1.jpg)

![](images/sample2.jpg)

- API key は読み取り専用のものです。Azure cosoms DB のアクセス流用に応じて制限することがあります。
- 整形済みの5年間のCSV形式のデータは <https://1drv.ms/u/s!AmXmBbuizQkXgpUYeqUVYd7WhAl4uw> にあるので活用してください。


# Author 

- tomoaki masuda 

# License

- MIT


