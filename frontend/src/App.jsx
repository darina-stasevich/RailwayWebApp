// App.jsx
import {BrowserRouter, Routes, Route, Navigate} from "react-router-dom";
import AuthorizationPage from "./pages/AuthorizationPage/AuthorizationPage.jsx";
import ProtectedRoute from "./routes/ProtectedRoute";
import FindRoutePage from "./pages/FindRoutePage/FindRoutePage.jsx";
import AdminPanel from "./pages/AdminPanel/AdminPanel.jsx";
import TicketBookingPage from "./pages/TicketBookingPage/TicketBookingPage.jsx";
import PassengersDetailsPage from "./pages/PassengersDetailsPage/PassengersDetailsPage.jsx";

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Главная страница перенаправляет на /login */}
                <Route path="/" element={<Navigate to="/login" replace />} />

                {/* Страница входа */}
                <Route path="/login" element={<AuthorizationPage />} />

                {/* Защищенные маршруты */}
                <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                    <Route path="/find-route" element={<FindRoutePage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                    <Route path="/tickets-booking" element={<TicketBookingPage />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                    <Route path="/admin-panel" element={<AdminPanel />} />
                </Route>

                <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                    <Route path="/passengers-details" element={<PassengersDetailsPage />} />
                </Route>

                {/* Обработка несуществующих путей */}
                <Route path="*" ele ment={<div>404 Not Found</div>} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;