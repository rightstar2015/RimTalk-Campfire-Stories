# RimTalk - Campfire Stories：0.2.0 Walkthrough

## 本輪狀態

已完成普通文化儀式、講述者面向聽眾、兩種選用記憶來源、英文 Mod 列表名稱與四語文件。2026-09-06 使用 RimWorld 1.6.4871 的獨立 quicktest 驗證三種安裝組合。Workshop 已獲准建立，僅設私人；公開發佈仍未獲授權。私人項目 ID：3796862048。2026-09-07 依作者實測更新文件、四語獨立欄位、三張展示圖與 GitHub 原始碼。

## 玩家流程

1. 啟用 Core、Ideology 與本模組。在文化中加入「戰功歌」，不會自動修改現有文化。
2. 建造並點燃原生篝火，四周留下集合及移動空間。
3. 從篝火開始儀式，選擇參與者。可指定最多三位講述者；留空則自動選擇 1～3 人。
4. 當輪讲述者到火邊，依實際聽眾中心決定朝向。以篝火為站位基準，優先選擇與聽眾相對的可達位置；沒有聽眾才朝篝火。
5. 無 AI 時每位進行普通講述／等待階段，不生成台詞、不提示缺少金鑰。輪替後依普通儀式品質取得心情結果。
6. AI 可用時增加文字對話：講述者開場、最多兩位聽眾接話、講述者收尾。沒有專屬動作指令、特殊獎勵、額外關係效果或主動記憶寫回。

## 自動相容

| 安裝組合 | 記憶與服務 |
|---|---|
| Core + Ideology + 本模組 | 獨立普通文化儀式，不載入 RimTalk 接口 |
| 加 RimTalk | 使用講述者自己的 RimTalk 內建對話歷史，只抽取對話內容；沿用 RimTalk 服務、模型及 API 設定 |
| 加 RimTalk 與 Expand Memory | 改讀講述者自己的 Expand Memory 記憶，不混用內建歷史 |
| AI 停用、未設定、忙碌逾時或請求失敗 | 回到普通儀式階段，不影響正常完成時的品質 |

Expand Memory 的衍生記憶元件亦可偵測。話題不限，日常、戰鬥、人際與紀念皆可。無可用記憶時，AI 聊當下，不捏造重大往事。RimTalk 的對話歷史由本體管理，重新啟動後可能為空。

## 設定與維護

保留講述者人數、接話人数、字數、逾時、近期記憶避免重複及專屬 Prompt。沒有獨立 API Key 欄位。移除舊版主動寫回選項，舊設定的該欄位不再使用。

外部名稱：RimTalk - Campfire Stories。保留 packageId sakicats.rimtalk.campfirestories、原 defName 及舊儀式名稱，減少既有識別變動。四語介面、玩家說明及 Workshop 介紹同步維護。

## 驗收結果與後續

- 已通過：兩個 DLL 編譯；27 XML 與四語各 39 欄驗證；遊戲載入九項定義檢查。
- 已通過：三種安裝組合各自三人輪替、站定面向人群、無 API 正常結束並產生心情結果。
- 已通過：RimTalk 個人歷史與 Expand Memory 個人記憶讀取；不混用來源，不抽取提示詞或其他人的紀錄。
- 作者實測通過（2026-09-07）：真實 API、完整存讀檔、自訂種族、大量模組環境及講述者面向人群。此處為作者回報，與上方自動測試分開記錄。
- 已知外部警告：現有 Expand Memory 英文檔有標籤不匹配與重複鍵，未修改其原始檔案。

## Steam Workshop 私人上架流程

1. 整理獨立發佈資料夾，只包含本模組的執行檔、定義、翻譯、封面及文件，不包含遊戲 DLL、前置模組、設定金鑰、玩家存檔或测试快取。
2. 英文主標題使用 RimTalk - Campfire Stories，版本 0.2.0。Ideology 為必要 DLC；RimTalk／Expand Memory 是選用整合，不設強制 Workshop 前置。
3. 採用第二版 640 × 360 Preview.png；1280 尺寸展示素材保留於 Artwork。640 × 360 為本專案使用的封面尺寸，非所有 Workshop 的統一強制尺寸。
4. 透過 Steam Workshop API 建立項目，保存 PublishedFileId；提交前明確設定 Private，提交後讀回可見性、標題及 ID。
5. 四語說明分別寫入 Steam 的 english、tchinese、schinese、japanese 欄位。繁中保留作者開場原句，簡中轉為簡體；英文、日文不含該句。加入需求模組連結與 GitHub／接手聲明。
6. 已有項目沿用相同 ID，更新前後確認維持 Private。
7. 私人頁面建立後檢查封面、名稱、介紹及下載內容。後續更新沿用同一 ID，避免重複建立。
8. 公開發佈須另有作者指示；本輪維持私人。

[Steam Workshop 上傳流程](https://partner.steamgames.com/doc/features/workshop/implementation) · [Steam 可見性設定](https://partner.steamgames.com/doc/api/ISteamUGC#SetItemVisibility)

## 封面保留

左側為狐耳側馬尾角色，中間保留服裝並加白色羽翼與光環，右側為黑髮鼠耳角色，搭配篝火。About/Preview.png 為第二版；原始與高清版本保留於 Artwork。本輪不重新生成圖像。

## 原始碼

https://github.com/rightstar2015/RimTalk-Campfire-Stories

本專案由作者於閒暇時間製作。若長期未更新，可留言告知作者後接手。

## 0.2.1：專屬圖標與說明整理

「戰功歌」改用篝火專屬透明圖標；既有存檔中沿用祭典圖標的儀式會在讀檔時更新。四語介紹的操作步驟與設定分開呈現，移除實測摘要及歷史／模型行為的非必要說明。Workshop 維持私人，保留三張展示圖。

## 0.2.2：韓語支援

新增韓語遊戲翻譯（39 欄）、韓語預設 Prompt 與韓文玩家說明；Workshop 韓文版參照英文版。這次只更新韓語支援與更新註記，保留現有公開狀態、封面及其他語言欄位。
