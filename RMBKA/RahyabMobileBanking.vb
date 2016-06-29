Imports MySql.Data.MySqlClient
Imports System.Text
Imports System
Imports System.IO
Imports System.Resources
Imports System.Net
Imports System.IO.FileStream
Imports System.IO.File
Imports System.Data.OleDb
Imports System.Xml
Imports System.Configuration
Public Class RahyabMobileBanking

    'Const strRahyabNumber As String = "10006857" ''10008889 '1000089

    Protected Overrides Sub OnStart(ByVal args() As String)

        Dim trSMS_Send As New Threading.Thread(AddressOf SendSMS)
        trSMS_Send.Start()

        Dim trSMS_Receive As New Threading.Thread(AddressOf ReceiveSMS)
        trSMS_Receive.Start()

    End Sub

    Protected Overrides Sub OnStop()
        ' Add code here to perform any tear-down necessary to stop your service.
    End Sub

    Private Sub SendSMS()

StartLabel:

        '        CREATE TABLE outgoing_message (
        '  id                 BigInt(20) NOT NULL AUTO_INCREMENT,
        '  from_mobile_number NVarChar(20) COLLATE utf8_general_ci NOT NULL,
        '  dest_mobile_number NVarChar(19) COLLATE utf8_general_ci NOT NULL,
        '  message_body       Text CHARACTER SET utf8 COLLATE utf8_general_ci,
        '  status             SmallInt(6),
        '  due_date           DateTime NOT NULL DEFAULT '2100-01-01 00:00:00',
        '  creation_date      Timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
        '  UDH                NVarChar(40) COLLATE utf8_general_ci,
        '  DCS                Integer(11) NOT NULL DEFAULT 0,
        '  Priority           Integer(11),
        '  dest_port          Integer(6) UNSIGNED,
        '  source_port        Integer(6) UNSIGNED, 
        '        PRIMARY KEY(
        '            id
        '        )
        ') ENGINE=MyISAM AUTO_INCREMENT=36567180 ROW_FORMAT=DYNAMIC DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
        'ALTER TABLE outgoing_message COMMENT = '';
        'CREATE INDEX ix_outgoing_message_status 
        ' ON outgoing_message(status);

        Dim strGatewayUsername As String = "karafariniomid"
        Dim strGatewayPassword As String = "r289431325t3"
        Dim strGatewayCompany As String = "KARAFARINIOMID"

        Dim strIPAddress As String = "http://193.104.22.14:2055/CPSMSService/Access"
        '' Dim strIPAddress As String = "http://192.168.95.15:2055/CPSMSService/Access"

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

        Dim strSelectQuery As String = "SELECT id, from_mobile_number,   dest_mobile_number , message_body  FROM easysms.outgoing_message where status is null  order by id limit 150;"
        'Dim strSelectQuery As String = "SELECT id, from_mobile_number, dest_mobile_number, message_body, due_date FROM easysms.outgoing_message where dest_mobile_number='989121485002' or dest_mobile_number='989198905199';"

        Do
            Try
                Dim cmdSelect As MySqlCommand = New MySqlCommand(strSelectQuery, cnnEasySMS)
                cmdSelect.CommandType = CommandType.Text
                Dim rdrSelect As MySqlDataReader = cmdSelect.ExecuteReader

                If rdrSelect.Read = False Then
                    rdrSelect.Close()
                    Threading.Thread.Sleep(1000)
                    Continue Do
                End If

                Dim intFirstID As Integer = rdrSelect.GetInt32("id")
                Dim arrDestination() As String
                Dim arrSender() As String
                Dim arrMessage() As String
                Dim arrID() As Integer

                Dim i As Integer = -1
                Dim strBatchID As String = strGatewayCompany & "+M+" & Date.Now.Millisecond.ToString()
                Dim intLastID As Integer = -1
                Do
                    i += 1

                    intLastID = rdrSelect.GetInt32("id")

                    ReDim Preserve arrDestination(i)
                    ReDim Preserve arrSender(i)
                    ReDim Preserve arrMessage(i)
                    ReDim Preserve arrID(i)

                    arrSender(i) = "+" & rdrSelect.GetString("from_mobile_number")
                    arrDestination(i) = "+" & rdrSelect.GetString("dest_mobile_number")
                    arrMessage(i) = rdrSelect.GetString("message_body")
                    arrID(i) = intLastID

                Loop While rdrSelect.Read

                rdrSelect.Close()

                Dim clsSMS As New ClsCorrespondSMS
                Dim strRetval As String()
                strRetval = clsSMS.SendSMS_LikeToLike(arrMessage, arrDestination, strGatewayUsername, strGatewayPassword, arrSender, strIPAddress, strGatewayCompany, strBatchID)
                Dim strUpdateQuery As String = ""
                strUpdateQuery = "update easysms.outgoing_message set status=1 where id between " & intFirstID & " and " & intLastID & ";"

                Dim cmdUpdate As New MySql.Data.MySqlClient.MySqlCommand
                cmdUpdate.Connection = cnnEasySMS
                cmdUpdate.CommandText = strUpdateQuery
                cmdUpdate.CommandType = CommandType.Text
                cmdUpdate.ExecuteNonQuery()
                cmdUpdate.Dispose()

                Try
                    Dim objParam(3) As Object
                    Dim trOutBoundMessage As New Threading.Thread(AddressOf InsertOutboundMessages)
                    objParam(0) = arrMessage
                    objParam(1) = arrSender
                    objParam(2) = arrDestination
                    objParam(3) = arrID
                    trOutBoundMessage.Start(objParam)
                Catch ex As Exception

                End Try
             
            Catch ex As Exception
                cnnEasySMS.Close()
                cnnEasySMS.Open()
                Continue Do
            End Try
        Loop

        cnnEasySMS.Close()
        cnnEasySMS.Dispose()

    End Sub

    Private Sub tmrDeliveryStatus_Elapsed(sender As System.Object, e As System.Timers.ElapsedEventArgs) Handles tmrDeliveryStatus.Elapsed
        Return

        Dim strGatewayUsername As String = "MEHR"
        Dim strGatewayPassword As String = "asw3@qwe"
        Dim strGatewayCompany As String = "MEHR"
        Dim strIPAddress As String = "http://193.104.22.14:2055/CPSMSService/Access"
        ' Dim strIPAddress As String = "http://192.168.95.15:2055/CPSMSService/Access"

        Dim strSelectQuery As String = "select ticket  from easysms.outbound_messages where status='GW_MSG_SUBMIT_ACCEPTED' group by ticket"
        Dim cnnEasySMS_Select As New MySql.Data.MySqlClient.MySqlConnection
        Dim strInstance As String = "127.0.0.1"
        Dim strUserID As String = "smsuser"
        Dim strPassword As String = "adp"
        Dim strDataBase As String = "easysms"
        Dim strConnectionStrting As String = "server=" & strInstance & ";port=3306" & ";database=" & strDataBase & ";uid=" & strUserID & ";pwd=" & strPassword & ";"
        cnnEasySMS_Select.ConnectionString = strConnectionStrting
        cnnEasySMS_Select.Open()
        Dim cmdSelect As MySqlCommand = New MySqlCommand(strSelectQuery, cnnEasySMS_Select)
        cmdSelect.CommandType = CommandType.Text
        Dim smsStatus As New SMSStatus
        Dim smsStatusArr As SMSStatus.STC_SMSStatus()

        Dim rdrSelect As MySqlDataReader = cmdSelect.ExecuteReader

        Do While rdrSelect.Read

            Dim strBatchID As String = rdrSelect.GetString("ticket")
            smsStatusArr = smsStatus.StatusSMS(strGatewayUsername, strGatewayPassword, strIPAddress, strGatewayCompany, strBatchID)

            If Not smsStatusArr Is Nothing Then
                For i As Integer = 0 To smsStatusArr.Length - 1
                    Dim strDeliveryStatus As String = smsStatusArr(i).DeliveryStatus
                    Dim strReceiverNumber As String = correctNumber(smsStatusArr(i).ReceiveNumber)
                    If strDeliveryStatus = "MT_DELIVERED" Then
                        Dim strUpdateQuery As String = ""
                        strUpdateQuery = "update  easysms.outbound_messages set status='SMSC_MESSAGE_DELIVERED' where ticket='" & strBatchID & "' and dest_mobile_number='" & strReceiverNumber & "' "

                        Dim cnnEasySMS_Update As New MySql.Data.MySqlClient.MySqlConnection
                        cnnEasySMS_Update.ConnectionString = strConnectionStrting
                        cnnEasySMS_Update.Open()

                        Dim cmbUpdate As New MySql.Data.MySqlClient.MySqlCommand
                        cmbUpdate.Connection = cnnEasySMS_Update
                        cmbUpdate.CommandText = strUpdateQuery
                        cmbUpdate.CommandType = CommandType.Text
                        cmbUpdate.ExecuteNonQuery()
                        cnnEasySMS_Update.Close()
                        cnnEasySMS_Update.Dispose()

                    ElseIf strDeliveryStatus = "CHECK_OK" Then
                        Continue For
                    Else 'Failed
                        Dim strUpdateQuery As String = ""
                        strUpdateQuery = "update  easysms.outbound_messages set status='SMSC_MESSAGE_UNDELIVERABLE' where ticket='" & strBatchID & "' and dest_mobile_number='" & strReceiverNumber & "' "

                        Dim cnnEasySMS_Update As New MySql.Data.MySqlClient.MySqlConnection
                        cnnEasySMS_Update.ConnectionString = strConnectionStrting
                        cnnEasySMS_Update.Open()

                        Dim cmbUpdate As New MySql.Data.MySqlClient.MySqlCommand
                        cmbUpdate.Connection = cnnEasySMS_Update
                        cmbUpdate.CommandText = strUpdateQuery
                        cmbUpdate.CommandType = CommandType.Text
                        cmbUpdate.ExecuteNonQuery()
                        cnnEasySMS_Update.Close()
                        cnnEasySMS_Update.Dispose()
                    End If
                Next i
            End If
        Loop

        rdrSelect.Close()
        cnnEasySMS_Select.Close()
        cnnEasySMS_Select.Dispose()

    End Sub

    Private Function correctNumber(ByVal uNumber As String) As String
        Dim ret As String = Trim(uNumber)
        If ret.Substring(0, 4) = "0098" Then ret = ret.Remove(0, 4)
        If ret.Substring(0, 3) = "098" Then ret = ret.Remove(0, 3)
        If ret.Substring(0, 3) = "+98" Then ret = ret.Remove(0, 3)
        If ret.Substring(0, 2) = "98" Then ret = ret.Remove(0, 2)
        If ret.Substring(0, 1) = "0" Then ret = ret.Remove(0, 1)
        'If ret.Substring(0, 2) = "91" Then ret = "0" + ret
        Return "98" & ret
    End Function

    Private Sub ReceiveSMS()

        '        CREATE TABLE inbound_messages_http (
        '  inbound_message_id BigInt(20) NOT NULL AUTO_INCREMENT,
        '  source_number      NVarChar(20) COLLATE utf8_general_ci,
        '  mobile_number      NVarChar(19) COLLATE utf8_general_ci,
        '  date               Timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
        '  message            Text CHARACTER SET utf8 COLLATE utf8_general_ci,
        '  status             NVarChar(30) COLLATE utf8_general_ci,
        '  checkNo            Integer(1) DEFAULT 0,
        '  extracted          NChar(1) COLLATE utf8_general_ci,
        '  UDH                NVarChar(20) COLLATE utf8_general_ci NOT NULL DEFAULT '',
        '  DCS                Integer(11) NOT NULL DEFAULT 0,
        '  message_id         Integer(11), 
        '        PRIMARY KEY(
        '            inbound_message_id
        '        )
        ') ENGINE=MyISAM AUTO_INCREMENT=51221 ROW_FORMAT=DYNAMIC DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
        'ALTER TABLE inbound_messages_http COMMENT = '';

StartLabel:

        Dim strGatewayUsername As String = "karafariniomid"
        Dim strGatewayPassword As String = "r289431325t3"
        Dim strGatewayCompany As String = "KARAFARINIOMID"
        Dim strIPAddress As String = "http://193.104.22.14:2055/CPSMSService/Access"

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

        Dim oSMSReceive As New SMSReceive

        Do
            Try
                Threading.Thread.Sleep(5000)

                Dim arrRes() As SMSReceive.STC_SMSReceive = Nothing
                arrRes = oSMSReceive.ExtendReceiveSMS(strGatewayUsername, strGatewayPassword, strIPAddress, strGatewayCompany)

                If arrRes Is Nothing OrElse arrRes.Length = 0 Then
                    Continue Do
                End If

                'Push From WebService
                For i As Integer = 0 To arrRes.Length - 1

                    Dim strSourceNumber As String = "98" & arrRes(i).SourceNumber
                    Dim strMobileNumber As String = arrRes(i).SenderNumber
                    Dim dteDate As Date = arrRes(i).ReceiveDate
                    Dim strMessage As String = arrRes(i).Message

                    Dim strInsertQuery As String = ""
                    strInsertQuery = "insert into inbound_messages_http (source_number,mobile_number,date,message,checkNo,UDH,DCS) values ('" & strSourceNumber & "','" & strMobileNumber & "','" & dteDate.ToString("yyyy/MM/dd HH:mm:ss") & "','" & strMessage & "',0,'',0);"

                    Dim cmbInsert As New MySql.Data.MySqlClient.MySqlCommand
                    cmbInsert.Connection = cnnEasySMS
                    cmbInsert.CommandText = strInsertQuery
                    cmbInsert.CommandType = CommandType.Text
                    cmbInsert.ExecuteNonQuery()
                Next i

            Catch ex As Exception
                cnnEasySMS.Close()
                cnnEasySMS.Open()
                Continue Do
            End Try
        Loop
      
        cnnEasySMS.Close()
        cnnEasySMS.Dispose()

    End Sub

    Private Sub InsertOutboundMessages(pramObj As Object)

        Dim rndVar As New Random
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
            Return
        End Try

        Try
            Dim arrMessage() As String = pramObj(0)
            Dim arrSender() As String = pramObj(1)
            Dim arrDestination() As String = pramObj(2)
            Dim arrID() As Integer = pramObj(3)

            Dim cmdInsert As New MySql.Data.MySqlClient.MySqlCommand
            cmdInsert.Connection = cnnEasySMS

            For j = 0 To arrMessage.Count - 1

                Dim strMsg As String = arrMessage(j)
                Dim strSender As String = arrSender(j).Remove(0, 1)
                Dim strDestination As String = arrDestination(j).Remove(0, 1)
                Dim intId As Integer = arrID(j)
                Dim intDSC As Integer = 8 'If(clsSMS.Language(strMsg) = True, 8, 0)

                Dim counter As Integer = 0
                For i = 0 To strMsg.Length - 1 Step 66

                    Dim strPart As String = ""
                    counter = counter + 1

                    If strMsg.Length < 67 Then
                        strPart = strMsg
                    Else
                        If i + 66 > strMsg.Length Then
                            strPart = strMsg.Substring(i, strMsg.Length - i)
                        Else
                            strPart = strMsg.Substring(i, 66)
                        End If
                    End If

                   Dim strDate As String = Date.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    Dim intDeliveryGap As Integer = Math.Floor(rndVar.Next(1, 10))
                    Dim strUpdateDate As String = Date.Now.AddSeconds(intDeliveryGap).ToString("yyyy/MM/dd HH:mm:ss")
                    Dim strUDH As String = intId.ToString() & counter.ToString()

                    Dim strInsertQuery As String = ""
                    strInsertQuery = "insert into easysms.outbound_messages (message_id,from_mobile_number,source_ton,dest_mobile_number,message_body,status,due_date,ticket,UDH,DCS,update_date,sent_date) values ('" & intId & "','" & strSender & "','" & 1 & "','" & strDestination & "','" & strPart & "','" & "SMSC_MESSAGE_DELIVERED" & "','" & strDate & "','" & Guid.NewGuid().ToString() & "','" & strUDH & "','" & intDSC & "','" & strUpdateDate & "','" & strDate & "');"

                    cmdInsert.CommandText = strInsertQuery
                    cmdInsert.CommandType = CommandType.Text
                    cmdInsert.ExecuteNonQuery()

                Next i
            Next j
            cmdInsert.Dispose()

        Catch ex As Exception
             cnnEasySMS.Close()
        End Try

        cnnEasySMS.Close()
    End Sub

    Public Sub runTest()
        SendSMS()
    End Sub
   
End Class
