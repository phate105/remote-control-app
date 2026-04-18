import React, { useState, useRef } from "react";
import {
  View, Text, TextInput, TouchableOpacity, StyleSheet, ScrollView,
  Alert, SafeAreaView, Platform, ActivityIndicator,
  Modal, FlatList, Image
} from "react-native";
import { StatusBar } from "expo-status-bar";
import { MaterialCommunityIcons } from "@expo/vector-icons";

export default function App() {
  const [ip, setIp] = useState("");
  const [connected, setConnected] = useState(false);
  const [connecting, setConnecting] = useState(false);
  const [isWaiting, setIsWaiting] = useState(false);
  const [response, setResponse] = useState("Hệ thống sẵn sàng. Vui lòng nhập IP...");
  const [socket, setSocket] = useState(null);
  const scrollViewRef = useRef();

  // Process
  const [showProcess, setShowProcess] = useState(false);
  const [processes, setProcesses] = useState([]);
  const [filtered, setFiltered] = useState([]);
  const [loadingProcess, setLoadingProcess] = useState(false);
  const loadingProcessRef = useRef(false);
  const [search, setSearch] = useState("");

  // Sysinfo
  const [showInfo, setShowInfo] = useState(false);
  const [sysInfo, setSysInfo] = useState(null);
  const [loadingInfo, setLoadingInfo] = useState(false);
  const loadingInfoRef = useRef(false);

  // Screenshot
  const [showScreenshot, setShowScreenshot] = useState(false);
  const [screenshotB64, setScreenshotB64] = useState(null);
  const [loadingScreenshot, setLoadingScreenshot] = useState(false);
  const loadingScreenshotRef = useRef(false);
  const [liveMode, setLiveMode] = useState(false);
  const liveModeRef = useRef(false);
  const liveIntervalRef = useRef(null);

  // Volume
  const [showVolume, setShowVolume] = useState(false);

  const addLog = (msg) => {
    const time = new Date().toLocaleTimeString();
    setResponse(prev => `${prev}\n[${time}] ${msg}`);
  };

  const connect = () => {
    if (!ip) return Alert.alert("Lỗi", "Vui lòng nhập IP máy tính!");
    setConnecting(true);
    addLog(`Đang kết nối tới ${ip}...`);
    if (socket) socket.close();

    const ws = new WebSocket(`ws://${ip}:8888`);

    ws.onopen = () => {
      setConnecting(false);
      setIsWaiting(true);
      addLog("📡 Đã thông tuyến. Đang chờ PC xác nhận...");
      setSocket(ws);
    };

    ws.onmessage = (e) => {
      try {
        const res = JSON.parse(e.data);

        // Auth
        if (res.status === "auth") {
          if (res.message === "ACCEPTED") {
            setIsWaiting(false);
            setConnected(true);
            addLog("✅ PC ĐÃ ĐỒNG Ý. Quyền điều khiển đã được cấp!");
          } else {
            addLog("❌ PC ĐÃ TỪ CHỐI KẾT NỐI!");
            ws.close();
          }
          return;
        }

        // Screenshot
        if (loadingScreenshotRef.current) {
          if (res.message && res.message !== "screenshot_error") {
            setScreenshotB64(res.message);
            setLoadingScreenshot(false);
            loadingScreenshotRef.current = false;
          } else {
            addLog("❌ Screenshot thất bại!");
            setShowScreenshot(false);
            loadingScreenshotRef.current = false;
          }
          return;
        }

        // Sysinfo
        if (loadingInfoRef.current) {
          const lines = res.message.split("\n").map(l => l.trim()).filter(l => l.length > 0);
          const info = {};
          lines.forEach(line => {
            if (line.includes("Tên máy:")) info.machine = line.split(":")[1]?.trim();
            if (line.includes("Tài khoản:")) info.user = line.split(":")[1]?.trim();
            if (line.includes("OS:")) info.os = line.split("OS:")[1]?.trim();
            if (line.includes("CPU:")) info.cpu = line.split("CPU:")[1]?.trim();
            if (line.includes("Màn hình:")) info.screen = line.split("Màn hình:")[1]?.trim();
            if (line.includes("Ổ C:")) info.disk = line.split("Ổ C:")[1]?.trim();
            if (line.includes(".NET")) info.dotnet = line.split(".NET Version:")[1]?.trim();
          });
          setSysInfo(info);
          setLoadingInfo(false);
          loadingInfoRef.current = false;
          return;
        }

        // Process list
        if (res.message && res.message.includes("tiến trình")) {
          const list = res.message.split("\n").map(p => p.trim()).filter(p => p.length > 0 && !p.startsWith("Danh sách") && !p.startsWith("Top")).sort();
          setProcesses(list);
          setFiltered(list);
          setLoadingProcess(false);
          loadingProcessRef.current = false;
          return;
        }

        addLog(`>> ${(res.status || "").toUpperCase()}: ${res.message}`);
      } catch {
        addLog(`>> ${e.data}`);
      }
    };

    ws.onerror = () => {
      setConnecting(false);
      setIsWaiting(false);
      addLog("❌ Lỗi kết nối! Kiểm tra IP hoặc Firewall.");
    };

    ws.onclose = () => {
      setConnected(false);
      setConnecting(false);
      setIsWaiting(false);
      setSocket(null);
      addLog("⏸️ Đã ngắt kết nối.");
    };
  };

  const sendCommand = (cmd) => {
    if (socket && connected) {
      socket.send(JSON.stringify({ command: cmd.toLowerCase(), data: {} }));
      addLog(`Gửi lệnh: ${cmd.toUpperCase()}`);
    } else {
      Alert.alert("Lỗi", "Chưa được cấp quyền điều khiển!");
    }
  };

  const confirmAction = (title, msg, cmd) => {
    Alert.alert(title, msg, [
      { text: "Hủy bỏ", style: "cancel" },
      { text: "Đồng ý", onPress: () => sendCommand(cmd), style: "destructive" }
    ]);
  };

  const openInfoModal = () => {
    setShowInfo(true);
    setLoadingInfo(true);
    loadingInfoRef.current = true;
    socket.send(JSON.stringify({ command: "getinfo", data: {} }));
  };

  const openProcessModal = () => {
    setShowProcess(true);
    setLoadingProcess(true);
    loadingProcessRef.current = true;
    setSearch("");
    socket.send(JSON.stringify({ command: "getprocess", data: {} }));
  };

  const openScreenshot = () => {
    setShowScreenshot(true);
    setScreenshotB64(null);
    setLoadingScreenshot(true);
    loadingScreenshotRef.current = true;
    socket.send(JSON.stringify({ command: "screenshot", data: {} }));
  };

  const toggleLive = (socketRef) => {
    if (liveModeRef.current) {
      // tắt live
      clearInterval(liveIntervalRef.current);
      liveModeRef.current = false;
      setLiveMode(false);
    } else {
      // bật live
      liveModeRef.current = true;
      setLiveMode(true);
      liveIntervalRef.current = setInterval(() => {
        if (liveModeRef.current && socketRef) {
          loadingScreenshotRef.current = true;
          socketRef.send(JSON.stringify({ command: "screenshot", data: {} }));
        }
      }, 2000);
    }
  };

  const handleSearch = (text) => {
    setSearch(text);
    setFiltered(processes.filter(p => p.toLowerCase().includes(text.toLowerCase())));
  };

  const killProcess = (name) => {
    Alert.alert("Kill Process", `Xác nhận kill "${name}"?`, [
      { text: "Hủy", style: "cancel" },
      {
        text: "Kill", style: "destructive",
        onPress: () => {
          socket.send(JSON.stringify({ command: "killprocess", data: { name } }));
          const updated = processes.filter(p => p !== name);
          setProcesses(updated);
          setFiltered(updated.filter(p => p.toLowerCase().includes(search.toLowerCase())));
          addLog(`Kill process: ${name}`);
        }
      }
    ]);
  };

  return (
    <SafeAreaView style={styles.container}>
      <StatusBar style="light" />

      {/* Main scroll để cuộn được toàn bộ */}
      <ScrollView contentContainerStyle={styles.scrollContent} keyboardShouldPersistTaps="handled">

        <View style={styles.header}>
          <MaterialCommunityIcons name="monitor-dashboard" size={32} color="#00E676" />
          <Text style={styles.title}>PC REMOTE</Text>
        </View>

        <View style={styles.card}>
          <Text style={styles.label}>ĐỊA CHỈ IP MÁY TÍNH</Text>
          <View style={styles.inputContainer}>
            <MaterialCommunityIcons name="lan" size={20} color="#888" style={styles.inputIcon} />
            <TextInput
              placeholder="192.168.1.15" placeholderTextColor="#555"
              value={ip} onChangeText={(text) => setIp(text.replace(/,/g, '.'))} style={styles.input}
              keyboardType="numeric" editable={!connected && !connecting && !isWaiting}
            />
          </View>
          <View style={styles.connectionActions}>
            {!connected && !isWaiting ? (
              <TouchableOpacity style={[styles.btn, styles.connectBtn, connecting && styles.disabledBtn]} onPress={connect} disabled={connecting}>
                {connecting ? <ActivityIndicator color="#000" /> : <><MaterialCommunityIcons name="lan-connect" size={20} color="#000" /><Text style={styles.btnTextBlack}>KẾT NỐI</Text></>}
              </TouchableOpacity>
            ) : (
              <TouchableOpacity style={[styles.btn, styles.disconnectBtn]} onPress={() => socket?.close()}>
                <MaterialCommunityIcons name="lan-disconnect" size={20} color="#FFF" />
                <Text style={styles.btnTextWhite}>{isWaiting ? "HỦY YÊU CẦU" : "NGẮT KẾT NỐI"}</Text>
              </TouchableOpacity>
            )}
          </View>
        </View>

        {isWaiting && !connected && (
          <View style={styles.waitingCard}>
            <ActivityIndicator size="large" color="#00E676" />
            <Text style={styles.waitingText}>ĐANG CHỜ PC XÁC NHẬN QUYỀN...</Text>
            <Text style={styles.waitingSubText}>Vui lòng nhấn "Yes" trên máy tính của bạn</Text>
          </View>
        )}

        {connected && (
          <View>
            <Text style={styles.sectionTitle}>NGUỒN HỆ THỐNG</Text>
            <View style={styles.dangerZone}>
              <TouchableOpacity style={[styles.actionBtn, styles.shutdownBtn]} onPress={() => confirmAction("Tắt máy", "Xác nhận TẮT máy tính?", "shutdown")}>
                <MaterialCommunityIcons name="power" size={28} color="#FFF" />
                <Text style={styles.actionText}>Tắt máy</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.actionBtn, styles.restartBtn]} onPress={() => confirmAction("Restart", "Xác nhận KHỞI ĐỘNG LẠI?", "restart")}>
                <MaterialCommunityIcons name="restart" size={28} color="#FFF" />
                <Text style={styles.actionText}>Restart</Text>
              </TouchableOpacity>
            </View>

            <Text style={styles.sectionTitle}>TIỆN ÍCH</Text>
            <View style={styles.utilitiesGrid}>
              <TouchableOpacity style={[styles.utilBtn, { backgroundColor: '#3949AB' }]} onPress={() => sendCommand("lock")}>
                <MaterialCommunityIcons name="lock" size={24} color="#FFF" /><Text style={styles.utilText}>Khóa</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.utilBtn, { backgroundColor: '#5E35B1' }]} onPress={() => confirmAction("Sleep", "Xác nhận cho máy vào chế độ NGỦ?", "sleep")}>
                <MaterialCommunityIcons name="power-sleep" size={24} color="#FFF" /><Text style={styles.utilText}>Sleep</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.utilBtn, { backgroundColor: '#00897B' }]} onPress={openInfoModal}>
                <MaterialCommunityIcons name="information" size={24} color="#FFF" /><Text style={styles.utilText}>Thông tin</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.utilBtn, { backgroundColor: '#F57C00' }]} onPress={openProcessModal}>
                <MaterialCommunityIcons name="format-list-bulleted" size={24} color="#FFF" /><Text style={styles.utilText}>Tiến trình</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.utilBtn, { backgroundColor: '#00838F' }]} onPress={openScreenshot}>
                <MaterialCommunityIcons name="monitor-screenshot" size={24} color="#FFF" /><Text style={styles.utilText}>Screenshot</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.utilBtn, { backgroundColor: '#E91E63' }]} onPress={() => setShowVolume(true)}>
                <MaterialCommunityIcons name="volume-high" size={24} color="#FFF" /><Text style={styles.utilText}>Volume</Text>
              </TouchableOpacity>
            </View>
          </View>
        )}

        {/* Terminal */}
        <View style={styles.terminalContainer}>
          <View style={styles.terminalHeader}>
            <Text style={styles.terminalTitle}>TERMINAL LOGS</Text>
            <TouchableOpacity onPress={() => setResponse("--- Terminal Cleared ---")}>
              <MaterialCommunityIcons name="trash-can-outline" size={18} color="#888" />
            </TouchableOpacity>
          </View>
          <ScrollView style={styles.terminalScroll} contentContainerStyle={{ paddingBottom: 10 }}
            ref={scrollViewRef} onContentSizeChange={() => scrollViewRef.current?.scrollToEnd({ animated: true })}>
            <Text style={styles.logText}>{response}</Text>
          </ScrollView>
        </View>

      </ScrollView>

      {/* ========== SYSINFO MODAL ========== */}
      <Modal visible={showInfo} animationType="slide" onRequestClose={() => setShowInfo(false)}>
        <SafeAreaView style={styles.modalContainer}>
          <View style={styles.modalHeader}>
            <TouchableOpacity onPress={() => setShowInfo(false)} style={styles.backBtn}>
              <MaterialCommunityIcons name="arrow-left" size={24} color="#00E676" />
            </TouchableOpacity>
            <Text style={styles.modalTitle}>THÔNG TIN HỆ THỐNG</Text>
            <TouchableOpacity onPress={openInfoModal}>
              <MaterialCommunityIcons name="refresh" size={24} color="#00E676" />
            </TouchableOpacity>
          </View>
          {loadingInfo ? (
            <View style={styles.center}><ActivityIndicator size="large" color="#00E676" /><Text style={styles.loadingText}>Đang tải...</Text></View>
          ) : sysInfo ? (
            <ScrollView contentContainerStyle={{ padding: 16, gap: 12 }}>
              {[
                { icon: "laptop", label: "Tên máy", value: sysInfo.machine, color: "#1976D2" },
                { icon: "account", label: "Tài khoản", value: sysInfo.user, color: "#7B1FA2" },
                { icon: "microsoft-windows", label: "Hệ điều hành", value: sysInfo.os, color: "#0288D1" },
                { icon: "cpu-64-bit", label: "CPU", value: sysInfo.cpu, color: "#F57C00" },
                { icon: "monitor", label: "Màn hình", value: sysInfo.screen, color: "#00897B" },
                { icon: "harddisk", label: "Ổ C", value: sysInfo.disk, color: "#C62828" },
                { icon: "dot-net", label: ".NET", value: sysInfo.dotnet, color: "#512DA8" },
              ].map(({ icon, label, value, color }) => (
                <View key={label} style={[styles.infoCard, { borderLeftColor: color }]}>
                  <MaterialCommunityIcons name={icon} size={24} color={color} />
                  <View style={{ marginLeft: 12, flex: 1 }}>
                    <Text style={styles.infoLabel}>{label}</Text>
                    <Text style={styles.infoValue}>{value || "—"}</Text>
                  </View>
                </View>
              ))}
            </ScrollView>
          ) : null}
        </SafeAreaView>
      </Modal>

      <Modal visible={showScreenshot} animationType="slide" onRequestClose={() => { setShowScreenshot(false); clearInterval(liveIntervalRef.current); liveModeRef.current = false; setLiveMode(false); }}>
        <SafeAreaView style={styles.modalContainer}>
          <View style={styles.modalHeader}>
            <TouchableOpacity onPress={() => { setShowScreenshot(false); clearInterval(liveIntervalRef.current); liveModeRef.current = false; setLiveMode(false); }} style={styles.backBtn}>
              <MaterialCommunityIcons name="arrow-left" size={24} color="#00E676" />
            </TouchableOpacity>
            <Text style={styles.modalTitle}>SCREENSHOT</Text>
            <View style={{ flexDirection: 'row', gap: 12 }}>
              <TouchableOpacity onPress={() => toggleLive(socket)}>
                <MaterialCommunityIcons name={liveMode ? "pause-circle" : "play-circle"} size={24} color={liveMode ? "#E53935" : "#00E676"} />
              </TouchableOpacity>
              <TouchableOpacity onPress={openScreenshot}>
                <MaterialCommunityIcons name="refresh" size={24} color="#00E676" />
              </TouchableOpacity>
            </View>
          </View>
          {liveMode && (
            <View style={{ backgroundColor: '#E53935', paddingVertical: 4, alignItems: 'center' }}>
              <Text style={{ color: '#FFF', fontSize: 11, fontWeight: 'bold' }}>🔴 LIVE — cập nhật mỗi 2 giây</Text>
            </View>
          )}
          {loadingScreenshot && !screenshotB64 ? (
            <View style={styles.center}>
              <ActivityIndicator size="large" color="#00E676" />
              <Text style={styles.loadingText}>Đang chụp màn hình...</Text>
            </View>
          ) : screenshotB64 ? (
            <ScrollView contentContainerStyle={{ padding: 16 }}>
              <Image
                source={{ uri: `data:image/jpeg;base64,${screenshotB64}` }}
                style={{ width: '100%', aspectRatio: 16 / 9, borderRadius: 12 }}
                resizeMode="contain"
              />
              <Text style={{ color: '#555', fontSize: 11, textAlign: 'center', marginTop: 8 }}>
                {liveMode ? "Live mode đang bật" : "Bấm ▶ để bật live mode"}
              </Text>
            </ScrollView>
          ) : null}
        </SafeAreaView>
      </Modal>

      {/* ========== PROCESS MODAL ========== */}
      <Modal visible={showProcess} animationType="slide" onRequestClose={() => setShowProcess(false)}>
        <SafeAreaView style={styles.modalContainer}>
          <View style={styles.modalHeader}>
            <TouchableOpacity onPress={() => setShowProcess(false)} style={styles.backBtn}>
              <MaterialCommunityIcons name="arrow-left" size={24} color="#00E676" />
            </TouchableOpacity>
            <Text style={styles.modalTitle}>TIẾN TRÌNH</Text>
            <TouchableOpacity onPress={() => { setLoadingProcess(true); loadingProcessRef.current = true; socket.send(JSON.stringify({ command: "getprocess", data: {} })); }}>
              <MaterialCommunityIcons name="refresh" size={24} color="#00E676" />
            </TouchableOpacity>
          </View>
          <View style={styles.searchContainer}>
            <MaterialCommunityIcons name="magnify" size={20} color="#888" />
            <TextInput placeholder="Tìm kiếm tiến trình..." placeholderTextColor="#555"
              value={search} onChangeText={handleSearch} style={styles.searchInput} />
          </View>
          <Text style={styles.countText}>{filtered.length} tiến trình</Text>
          {loadingProcess ? (
            <View style={styles.center}><ActivityIndicator size="large" color="#00E676" /><Text style={styles.loadingText}>Đang tải...</Text></View>
          ) : (
            <FlatList
              data={filtered}
              keyExtractor={(item, i) => `${item}-${i}`}
              renderItem={({ item }) => (
                <View style={styles.processItem}>
                  <MaterialCommunityIcons name="application" size={16} color="#00E676" style={{ marginRight: 10 }} />
                  <Text style={styles.processName} numberOfLines={1}>{item}</Text>
                  <TouchableOpacity onPress={() => killProcess(item)} style={styles.killBtn}>
                    <MaterialCommunityIcons name="close-circle" size={20} color="#E53935" />
                  </TouchableOpacity>
                </View>
              )}
              ItemSeparatorComponent={() => <View style={{ height: 1, backgroundColor: '#1E1E1E' }} />}
              contentContainerStyle={{ paddingBottom: 30 }}
            />
          )}
        </SafeAreaView>
      </Modal>

      {/* ========== VOLUME MODAL ========== */}
      <Modal visible={showVolume} animationType="slide" transparent={true} onRequestClose={() => setShowVolume(false)}>
        <View style={styles.volumeModalOverlay}>
          <View style={styles.volumeModalContent}>
            <View style={styles.modalHeaderSmall}>
              <Text style={styles.modalTitleSmall}>ĐIỀU KHIỂN ÂM THANH</Text>
              <TouchableOpacity onPress={() => setShowVolume(false)}>
                <MaterialCommunityIcons name="close" size={24} color="#FFF" />
              </TouchableOpacity>
            </View>
            <View style={styles.volumeGrid}>
              <TouchableOpacity style={[styles.volBtn, { backgroundColor: '#4CAF50' }]} onPress={() => sendCommand("volup")}>
                <MaterialCommunityIcons name="volume-plus" size={32} color="#FFF" />
                <Text style={styles.volText}>Tăng âm</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.volBtn, { backgroundColor: '#8BC34A' }]} onPress={() => sendCommand("voldown")}>
                <MaterialCommunityIcons name="volume-minus" size={32} color="#FFF" />
                <Text style={styles.volText}>Giảm âm</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.volBtn, { backgroundColor: '#F44336' }]} onPress={() => sendCommand("volmute")}>
                <MaterialCommunityIcons name="volume-mute" size={32} color="#FFF" />
                <Text style={styles.volText}>Mute</Text>
              </TouchableOpacity>
              <TouchableOpacity style={[styles.volBtn, { backgroundColor: '#2196F3' }]} onPress={() => sendCommand("playpause")}>
                <MaterialCommunityIcons name="play-pause" size={32} color="#FFF" />
                <Text style={styles.volText}>Play/Pause</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>

    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#121212' },
  scrollContent: { padding: 20, paddingTop: Platform.OS === 'android' ? 40 : 20, paddingBottom: 40 },
  header: { flexDirection: 'row', alignItems: 'center', marginBottom: 20, justifyContent: 'center' },
  title: { fontSize: 24, fontWeight: '800', color: '#00E676', marginLeft: 10, letterSpacing: 1 },
  card: { backgroundColor: '#1E1E1E', borderRadius: 16, padding: 20, elevation: 8, marginBottom: 15 },
  label: { color: '#888', fontSize: 11, fontWeight: 'bold', marginBottom: 8, letterSpacing: 1 },
  inputContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#2C2C2C', borderRadius: 10, paddingHorizontal: 12, marginBottom: 16 },
  inputIcon: { marginRight: 8 },
  input: { flex: 1, color: '#FFF', fontSize: 18, paddingVertical: 12, fontWeight: '600' },
  connectionActions: { flexDirection: 'row' },
  btn: { flex: 1, flexDirection: 'row', padding: 14, borderRadius: 10, alignItems: 'center', justifyContent: 'center', gap: 8 },
  connectBtn: { backgroundColor: '#00E676' },
  disconnectBtn: { backgroundColor: '#E53935' },
  disabledBtn: { backgroundColor: '#555' },
  btnTextBlack: { color: '#000', fontWeight: 'bold', fontSize: 14 },
  btnTextWhite: { color: '#FFF', fontWeight: 'bold', fontSize: 14 },
  waitingCard: { backgroundColor: '#1E1E1E', padding: 30, borderRadius: 16, alignItems: 'center', marginBottom: 20, borderWidth: 1, borderColor: '#333' },
  waitingText: { color: '#00E676', marginTop: 15, fontWeight: 'bold', fontSize: 14 },
  waitingSubText: { color: '#888', fontSize: 12, marginTop: 5 },
  sectionTitle: { color: '#888', fontSize: 11, fontWeight: 'bold', marginBottom: 10, marginTop: 15, letterSpacing: 1 },
  dangerZone: { flexDirection: 'row', gap: 12, marginBottom: 10 },
  actionBtn: { flex: 1, padding: 20, borderRadius: 16, alignItems: 'center', gap: 8 },
  shutdownBtn: { backgroundColor: '#D32F2F' },
  restartBtn: { backgroundColor: '#1976D2' },
  actionText: { color: '#FFF', fontWeight: 'bold', fontSize: 13 },
  utilitiesGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12, marginBottom: 10 },
  utilBtn: { flexBasis: '30%', padding: 12, borderRadius: 12, alignItems: 'center', gap: 4 },
  utilText: { color: '#FFF', fontWeight: '600', fontSize: 11, textAlign: 'center' },
  terminalContainer: { height: 200, backgroundColor: '#0A0A0A', borderRadius: 12, borderWidth: 1, borderColor: '#333', marginTop: 15, overflow: 'hidden' },
  terminalHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', backgroundColor: '#1A1A1A', padding: 8, paddingHorizontal: 12 },
  terminalTitle: { color: '#666', fontSize: 10, fontWeight: 'bold' },
  terminalScroll: { padding: 12 },
  logText: { color: '#00FF00', fontFamily: Platform.OS === 'ios' ? 'Menlo' : 'monospace', fontSize: 11, lineHeight: 16 },
  modalContainer: { flex: 1, backgroundColor: '#121212' },
  modalHeader: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', padding: 20, paddingTop: Platform.OS === 'android' ? 40 : 10 },
  backBtn: { padding: 4 },
  modalTitle: { fontSize: 18, fontWeight: '800', color: '#00E676', letterSpacing: 1 },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#1E1E1E', marginHorizontal: 16, marginBottom: 8, borderRadius: 10, paddingHorizontal: 12 },
  searchInput: { flex: 1, color: '#FFF', fontSize: 14, paddingVertical: 10, marginLeft: 8 },
  countText: { color: '#555', fontSize: 11, marginLeft: 16, marginBottom: 8 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center' },
  loadingText: { color: '#888', marginTop: 12 },
  processItem: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: 16, paddingVertical: 12 },
  processName: { flex: 1, color: '#EEE', fontSize: 13, fontFamily: 'monospace' },
  killBtn: { padding: 4 },
  infoCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: '#1E1E1E', borderRadius: 12, padding: 16, borderLeftWidth: 4, marginBottom: 10 },
  infoLabel: { color: '#888', fontSize: 11, fontWeight: 'bold', letterSpacing: 1 },
  infoValue: { color: '#FFF', fontSize: 14, marginTop: 2, fontWeight: '600' },
  volumeModalOverlay: { flex: 1, backgroundColor: 'rgba(0,0,0,0.7)', justifyContent: 'flex-end' },
  volumeModalContent: { backgroundColor: '#1E1E1E', borderTopLeftRadius: 20, borderTopRightRadius: 20, padding: 20, paddingBottom: Platform.OS === 'ios' ? 40 : 20 },
  modalHeaderSmall: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 },
  modalTitleSmall: { fontSize: 16, fontWeight: 'bold', color: '#00E676', letterSpacing: 1 },
  volumeGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: 12 },
  volBtn: { flexBasis: '47%', flexGrow: 1, padding: 20, borderRadius: 16, alignItems: 'center', gap: 8 },
  volText: { color: '#FFF', fontWeight: 'bold', fontSize: 13 },
});