Imports MySql.Data.MySqlClient
Public Class Form1

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Dim obj As New RahyabMobileBanking
        obj.runTest()

        ''Dim cnnEasySMS As New MySql.Data.MySqlClient.MySqlConnection
        ''Dim strInstance As String = "127.0.0.1"
        ''Dim strUserID As String = "smsuser"
        ''Dim strPassword As String = "adp"
        ''Dim strDataBase As String = "easysms"
        ''Dim strConnectionStrting As String = "server=" & strInstance & ";port=3306" & ";database=" & strDataBase & ";uid=" & strUserID & ";pwd=" & strPassword & ";"
        ''cnnEasySMS.ConnectionString = strConnectionStrting

        ''Try
        ''    cnnEasySMS.Open()
        ''Catch ex As Exception
        ''    Threading.Thread.Sleep(1000)
        ''End Try

        ''Dim strSelectQuery As String = "SELECT id, from_mobile_number, dest_mobile_number, message_body, due_date FROM easysms.outgoing_message where dest_mobile_number='989121485002' or dest_mobile_number='989198905199';"

        ''Try
        ''    Dim cmdSelect As MySqlCommand = New MySqlCommand(strSelectQuery, cnnEasySMS)
        ''    cmdSelect.CommandType = CommandType.Text
        ''    Dim rdrSelect As MySqlDataReader = cmdSelect.ExecuteReader

        ''    If rdrSelect.Read = False Then
        ''        rdrSelect.Close()
        ''        Threading.Thread.Sleep(1000)
        ''    End If

        ''    Dim intFirstID As Integer = rdrSelect.GetInt32("id")
        ''    'Dim arrDestination() As String
        ''    'Dim arrSender() As String
        ''    'Dim arrMessage() As String
        ''    'Dim i As Integer = -1
        ''    'Dim intLastID As Integer = -1

        ''    Dim clsSMS As New ClsCorrespondSMS
        ''    Dim strMsg As String = rdrSelect.GetString("message_body")
        ''    Dim strSender As String = rdrSelect.GetString("from_mobile_number")
        ''    Dim strDestination As String = rdrSelect.GetString("dest_mobile_number")
        ''    Dim intDSC As Integer = If(clsSMS.Language(strMsg) = True, 8, 0)
        ''    rdrSelect.Close()

        ''    For i = 0 To strMsg.Length - 1 Step 66

        ''        Dim strPart As String = ""
        ''        If strMsg.Length < 67 Then
        ''            strPart = strMsg
        ''        Else
        ''            If i + 66 > strMsg.Length Then
        ''                strPart = strMsg.Substring(i, strMsg.Length - i)
        ''            Else
        ''                strPart = strMsg.Substring(i, 66)
        ''            End If
        ''        End If

        ''        Dim strInsertQuery As String = ""
        ''        strInsertQuery = "insert into easysms.outbound_messages (message_id,from_mobile_number,source_ton,dest_mobile_number,message_body,status,due_date,DCS) values ('" & intFirstID & "','" & strSender & "','" & 1 & "','" & strDestination & "','" & strPart & "','" & "SMSC_MESSAGE_DELIVERED" & "','" & Date.Now.ToString("yyyy/MM/dd HH:mm:ss") & "','" & intDSC & "');"

        ''        Dim cmbInsert As New MySql.Data.MySqlClient.MySqlCommand
        ''        cmbInsert.Connection = cnnEasySMS
        ''        cmbInsert.CommandText = strInsertQuery
        ''        cmbInsert.CommandType = CommandType.Text
        ''        cmbInsert.ExecuteNonQuery()

        ''    Next i

        ''Catch ex As Exception
        ''    Threading.Thread.Sleep(1000)
        ''End Try


    End Sub

End Class