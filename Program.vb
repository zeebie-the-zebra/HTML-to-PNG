Imports PuppeteerSharp
Imports System.IO
Imports System.Threading.Tasks

Module Module1

    Sub Main()
        Dim currentDirectory As String = Directory.GetCurrentDirectory()
        Dim htmlDirectory As String = Path.Combine(currentDirectory, "html")
        Dim outputDirectory As String = Path.Combine(currentDirectory, "png")

        If Not Directory.Exists(htmlDirectory) Then
            Console.WriteLine("HTML directory does not exist.")
            Return
        End If

        If Not Directory.Exists(outputDirectory) Then
            Directory.CreateDirectory(outputDirectory)
        End If

        MainAsync(htmlDirectory, outputDirectory).GetAwaiter().GetResult()
    End Sub

    Private Async Function MainAsync(htmlDirectory As String, outputDirectory As String) As Task
        Try
            ' Configure PuppeteerSharp to download Chromium
            Dim browserFetcher = New BrowserFetcher(New BrowserFetcherOptions With {
            .Path = Path.Combine(Directory.GetCurrentDirectory(), "puppeteer-sharp")
        })

            ' Ensure latest version of Chromium is downloaded
            Dim revisionInfo = Await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision)

            ' Launch Puppeteer
            Dim launchOptions As New LaunchOptions With {
            .Headless = True,
            .ExecutablePath = revisionInfo.ExecutablePath
        }

            ' Get all HTML files in the directory
            Dim htmlFiles As String() = Directory.GetFiles(htmlDirectory, "*.html")

            ' Process each HTML file
            For Each htmlFile As String In htmlFiles
                Dim browser = Await Puppeteer.LaunchAsync(launchOptions)
                Await ConvertHtmlToImageAsync(browser, htmlFile, outputDirectory)
            Next

            Console.WriteLine("Batch conversion completed.")
            Console.ReadLine()

        Catch ex As Exception
            Console.WriteLine("Error: " & ex.Message)
        End Try
    End Function


    Private Async Function ConvertHtmlToImageAsync(browser As Browser, htmlFilePath As String, outputDirectory As String) As Task
        Try
            ' Create a new page
            Dim page = Await browser.NewPageAsync()

            ' Navigate to the HTML file
            Dim uri = New Uri(htmlFilePath)
            Await page.GoToAsync(uri.ToString())

            ' Wait for the page content to load
            Await page.WaitForSelectorAsync("body") ' Example: wait for body element to appear

            ' Capture a full-page screenshot of the page
            Dim screenshotPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(htmlFilePath) & ".png")
            Dim screenshotOptions As New ScreenshotOptions With {
            .Type = ScreenshotType.Png,
            .FullPage = True ' Capture full page screenshot
        }
            Await page.ScreenshotAsync(screenshotPath, screenshotOptions)

            Console.WriteLine("Converted: " & htmlFilePath)

            ' Close the page
            Await page.CloseAsync()

            ' Close the browser instance
            Await browser.CloseAsync()

        Catch ex As Exception
            Console.WriteLine("Error converting HTML to image (" & htmlFilePath & "): " & ex.Message)
        End Try
    End Function

End Module
