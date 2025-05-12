// App.jsx
import {BrowserRouter, Routes, Route, Navigate} from "react-router-dom";
import Login from "./components/Login/Login.jsx";
import ProtectedRoute from "./routes/ProtectedRoute";
import FindRoutePage from "./pages/FindRoutePage.jsx";
import AdminPanel from "./pages/AdminPanel.jsx";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Главная страница перенаправляет на /login */}
                <Route path="/" element={<Navigate to="/login" replace />} />

                {/* Страница входа */}
                <Route path="/login" element={<Login />} />

                {/* Защищенные маршруты */}
                <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                    <Route path="/find-route" element={<FindRoutePage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                    <Route path="/admin-panel" element={<AdminPanel />} />
                </Route>

                {/* Обработка несуществующих путей */}
                <Route path="*" element={<div>404 Not Found</div>} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;