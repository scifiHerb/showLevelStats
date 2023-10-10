# showLevelStats
![Beat Saber 2023_10_10 15_47_19](https://github.com/scifiHerb/showLevelStats/assets/109839172/b0063ce9-58e0-4222-9f6a-125bdc4ef9ae)
![Beat Saber 2023_09_11 16_54_29](https://github.com/scifiHerb/showLevelStats/assets/109839172/cd6976b3-aaa9-46a6-8084-59212a3e017d)

# 説明
プレイボタンの下に譜面のBSRや日時、Vote数などの情報を表示するMODです。
付加的な機能として BSR部分をクリックすると譜面と作者の詳細情報を表示できます。
譜面や作者の説明文はGoogle翻訳を通じて翻訳する機能をつけています、ModSettingsで言語設定が出来ますが取得URLに国コードをつけているだけなので直接設定ファイルをいじればどの言語でも対応できます。

# ChangeLog  
2023-10-10  
・ReactionTime、JumpDistanceなどの表示を追加  
・テキストの設定方式を、文字列の指定した場所に変数を割り当てる方式に変更  
・テキストUIの表示位置をオフセットで設定できるようにしました。  

2023-9-20  
・ModSettingsの追加  
・言語設定、自動翻訳の追加  

2023-9-13  
・翻訳機能を追加しました  
・Settingsを作りました、翻訳先の言語等設定できます。  

2023-9-7  
・BSRをクリックすると譜面の詳細情報が表示されるようにしました。  
・マルチのときの位置ずれ少し改善しました

# 不具合
・Description取得時スクロールバーが更新されない（再更新で直ります)  
・現在最新版2.5で突然表示が消滅するバグがある（表示位置にポインタを当てると出る）との報告を受けました、現在対応中です

# 設定項目 
{  
	"translateCountry":"ja",翻訳先の国コードです、google翻訳のURLの国コードと同じです。  
	"autoTranslate":false,自動翻訳をするかの設定です。  
 "textConfig":"{BSR}(↑<color=#00ff00>{upvotes}</color>:↓<color=#ff0000>{downvotes}</color>)\n{uploadDate}",変数を文字列に差し込む形で表示テキストを設定できます。  
 "sample1,2",表示テキストのサンプル類です、1はBSR,Votes、日時、2はそれに加えてJDとRT追記しています。  
 "offset~",テキストの表示位置のオフセットです。  
}

# 設定方法
下記のダブルクォーテーションに囲まれた文字列をその変数に置き換えその他の文字列はそのままにします。  
改行などは通常のエスケープシーケンスで行えます。   

またUnityのRichTextを使用しているので書式設定、色設定などそのまま入力することで反映させることが出来ます。  
# 変数類
"{BSR}",   
"{upvotes}",   
"{downvotes}",   
"{uploadDate}",   
"{JD}",JumpDistance  
"{RT}",ReactionTime  
"{NJS}",noteJumpMovementSpeed  
"{ranked}",   
"{totalPlays}",   
"{downloads}",   
"{hash}",   
"{name}",   
"{description}",   
