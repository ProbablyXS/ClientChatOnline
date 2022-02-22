Imports System.Net
Imports System.Net.Sockets
Imports System.Text

Public Class MainForm

    Dim connected As Boolean = False
    Dim timer As Integer = 3 ' = 5 secondes
    Dim _client As New TcpClient

    Public Async Function ConnectToTheServer() As Task

        If connected = True Then
            Return
        End If

        connected = True

        Await Task.Delay(1)

        Try

            Dim ip As String = IpTextBox.Text
            Dim port As Integer = 5656

            _client = New TcpClient
            Await _client.ConnectAsync(ip, port)

            CheckForIllegalCrossThreadCalls = False

            Threading.ThreadPool.QueueUserWorkItem(AddressOf ReceiveMessages) 'Start ReceReceiveMessages function


            'SEND CLIENT CONNECTION MESSAGE
            Dim Clientconnected As String = UsernameTextBox.Text
            Dim ns As NetworkStream = _client.GetStream()

            ns.Write(Encoding.UTF8.GetBytes(Clientconnected), 0, Clientconnected.Length)

            ButtonConnect.Enabled = False
            'END SEND CLIENT CONNECTION MESSAGE

        Catch ex As Exception

            RichTextBox1.Text = "Cannot connect from the server..."

            ButtonConnect.Enabled = True
            connected = False
            AutoReconnect()
            Return

        End Try

    End Function

    Public Sub SendMessage(Username As String, Message As String)

        If connected = False Then
            Return
        End If

        Try
            Dim ns As NetworkStream = _client.GetStream()
            Dim text As String = Username & " : " & Message
            ns.Write(Encoding.UTF8.GetBytes(text), 0, text.Length)
            MessageTextBox.Clear()

        Catch ex As Exception
            RichTextBox1.Text = "Cannot receive information from the server..."
            ButtonConnect.Enabled = True
            connected = False
            AutoReconnect()
        End Try


    End Sub

    Private Async Sub ReceiveMessages(state As Object)


        Try
            While True

                Dim ns As NetworkStream = _client.GetStream()
                Dim toReceive(100000) As Byte
                ns.Read(toReceive, 0, toReceive.Length)
                Dim txt As String = Encoding.UTF8.GetString(toReceive)

                Await Task.Delay(100)
                If RichTextBox1.TextLength > 0 Then
                    RichTextBox1.Text &= vbNewLine & txt
                Else
                    RichTextBox1.Text = txt
                End If

                'SCROLL TO THE END
                RichTextBox1.SelectionStart = RichTextBox1.Text.Length
                RichTextBox1.ScrollToCaret()
                'END SCROLL TO THE END

            End While
        Catch ex As Exception

            RichTextBox1.Text = "Server closed..."
            ButtonConnect.Enabled = True
            connected = False
            AutoReconnect()

        End Try


    End Sub



    Private Sub MessageTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles MessageTextBox.KeyDown

        If (e.KeyCode = Keys.Enter) Then
            e.SuppressKeyPress = True
            SendMessage(UsernameTextBox.Text, MessageTextBox.Text)
        End If

    End Sub

    Private Sub ButtonSend_Click(sender As Object, e As EventArgs) Handles ButtonSend.Click
        SendMessage(UsernameTextBox.Text, MessageTextBox.Text)
    End Sub

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim hostName As String = Dns.GetHostName()
        IpTextBox.Text = Dns.GetHostByName(hostName).AddressList(0).ToString()
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
        FontDialog1.ShowDialog()
        RichTextBox1.Font = FontDialog1.Font
        MessageTextBox.Font = FontDialog1.Font
    End Sub

    Public Async Sub AutoReconnect()

        If CheckBox1.Checked = False Or connected = True Then
            Return
        Else
            connected = True
        End If

        While True
            Await Task.Delay(1)
            If timer = 0 Then
                timer = 3 ' = 5 secondes
                RichTextBox1.Text = "Connecting"
                connected = False
                Await ConnectToTheServer()
                Return
            Else
                RichTextBox1.Text = "Trying to reconnect from the server: " & timer
                timer -= 1
                Await Task.Delay(1000)
            End If
        End While

    End Sub

    Private Async Sub ButtonConnect_Click(sender As Object, e As EventArgs) Handles ButtonConnect.Click
        Await ConnectToTheServer()
    End Sub
End Class
