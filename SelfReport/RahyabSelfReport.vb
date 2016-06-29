Imports MySql.Data.MySqlClient

Public Class RahyabSelfReport

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Add code here to start your service. This method should set things
        ' in motion so your service can do its work.
    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
    End Sub

    Private Sub tmrReport_Elapsed(sender As System.Object, e As System.Timers.ElapsedEventArgs) Handles tmrReport.Elapsed
        ReportSend()
    End Sub

    Public Sub ReportSend()

StartLabel:
        Dim clsSMS As New clsSMS
        Dim strRetval As String()

        Dim strGatewayUsername As String = "karafariniomid"
        Dim strGatewayPassword As String = "r289431325t3"
        Dim strGatewayCompany As String = "KARAFARINIOMID"
        Dim strBatchID As String = strGatewayCompany & "+F+" & Date.Now.Millisecond.ToString()
        Dim strIPAddress As String = "http://193.104.22.14:2055/CPSMSService/Access"

        Dim intHour As Integer = Date.Now.Hour
        'clsSMS.SendSMS_Single("آغاز سرویس " & GetPersianDate(Date.Now), "09121485002", "smshome0", "admin@0001", "10001", strIPAddress, "SMSONLINE", True, False, False, "SMSONLINE+T+" & Date.Now.Millisecond.ToString())

        Dim cnnEasySMS As New MySql.Data.MySqlClient.MySqlConnection
        Dim strInstance As String = "127.0.0.1"
        Dim strUserID As String = "smsuser"
        Dim strPassword As String = "adp"
        Dim strDataBase As String = "easysms"
        Dim strConnectionStrting As String = "server=" & strInstance & ";port=3306" & ";database=" & strDataBase & ";uid=" & strUserID & ";pwd=" & strPassword & ";"
        cnnEasySMS.ConnectionString = strConnectionStrting

        Try
            cnnEasySMS.Open()
        Catch ex As Exception
            Threading.Thread.Sleep(1000)
            GoTo StartLabel
        End Try

        Try

            Dim strSelectQuery As String = "SELECT count(*) AS cntSMS FROM easysms.outgoing_message where status is null ;"
            Dim cmdSelect As MySqlCommand = New MySqlCommand(strSelectQuery, cnnEasySMS)
            cmdSelect.CommandType = CommandType.Text
            Dim rdrSelect As MySqlDataReader = cmdSelect.ExecuteReader

            If rdrSelect.Read = False Then
                rdrSelect.Close()
                Threading.Thread.Sleep(1000)
                GoTo StartLabel
            End If

            Dim intFailedCnt As Integer = rdrSelect.GetInt32("cntSMS")
            rdrSelect.Close()

            Dim strMsg As String = ""
            Dim strDestination As String() = Nothing

            If intFailedCnt > 200 AndAlso intHour >= 7 AndAlso intHour <= 22 Then

                strMsg = "گزارش سرویس پایش مورخ " & GetPersianDate(Date.Now) & vbCrLf & "تعداد " & intFailedCnt & " پیامک ارسال نشده است. احتمالاً مشکل در خطوط ارتباطی سرور می باشد."
                ReDim strDestination(3)
                strDestination(0) = "09121485002"
                strDestination(1) = "09121576554"
                strDestination(2) = "09122764983"
                strDestination(3) = "09126133721"

            ElseIf intFailedCnt <= 200 AndAlso intHour = 10 Then
                strMsg = "گزارش سرویس پایش مورخ " & GetPersianDate(Date.Now) & vbCrLf & "سرویس پایش پیامک صندوق امید فعال است و هیچ پیامکی در صف ارسال نمی باشد."
                ReDim strDestination(4)
                strDestination(0) = "09121485002"
                strDestination(1) = "09121576554"
                strDestination(2) = "09122764983"
                strDestination(3) = "09125010426"
                strDestination(4) = "09123384043"
            End If

            Dim strStatus As String = ""
            If strMsg <> "" AndAlso strDestination.Count > 0 Then
                strRetval = clsSMS.SendSMS_Batch(strMsg, strDestination, strGatewayUsername, strGatewayPassword, "10006858", strIPAddress, strGatewayCompany, True, False, False, strBatchID)
                If strRetval(0) = "CHECK_OK" Then
                    strStatus = "SMS Sent Successfully"
                Else
                    strStatus = strRetval(1)
                End If

                Dim strInsertQuery As String = ""
                Dim strDate As String = Date.Now.ToString("yyyy/MM/dd")
                Dim strSDate As String = Date.Now.ToString("yyyy/MM/dd HH:mm:ss")
                strInsertQuery = "insert into easysms.report_log (report_date, status, message_text, due_date) values ('" & strDate & "','" & strStatus & "','" & strMsg & "','" & strSDate & "');"

                Dim cmdInsert As New MySql.Data.MySqlClient.MySqlCommand
                cmdInsert.Connection = cnnEasySMS
                cmdInsert.CommandText = strInsertQuery
                cmdInsert.CommandType = CommandType.Text
                cmdInsert.ExecuteNonQuery()
                cmdInsert.Dispose()
            End If

        Catch ex As Exception
            Dim strInsertQuery As String = ""
            Dim strDate As String = Date.Now.ToString("yyyy/MM/dd")
            Dim strSDate As String = Date.Now.ToString("yyyy/MM/dd HH:mm:ss")
            strInsertQuery = "insert into easysms.report_log (report_date, status, message_text, due_date) values ('" & strDate & "','" & "An Error Occured." & "','" & ex.Message & "','" & strSDate & "');"

            Dim cmdInsert As New MySql.Data.MySqlClient.MySqlCommand
            cmdInsert.Connection = cnnEasySMS
            cmdInsert.CommandText = strInsertQuery
            cmdInsert.CommandType = CommandType.Text
            cmdInsert.ExecuteNonQuery()
            cmdInsert.Dispose()
        End Try

    End Sub

    Public Function GetPersianDate(ByVal dteGregorain As Date) As String

        Dim persCal As New Globalization.PersianCalendar
        Return persCal.GetYear(dteGregorain).ToString("0000") & "/" & persCal.GetMonth(dteGregorain).ToString("00") & "/" & persCal.GetDayOfMonth(dteGregorain).ToString("00") & " " & dteGregorain.ToString("HH:mm")


    End Function
End Class
