import {BrowserRouter, Routes, Route, Navigate} from "react-router-dom";
import AuthorizationPage from "./pages/AuthorizationPage/AuthorizationPage.jsx";
import ProtectedRoute from "./routes/ProtectedRoute";
import FindRoutePage from "./pages/FindRoutePage/FindRoutePage.jsx";
import AdminPanel from "./pages/AdminPanel/AdminPanel.jsx";
import TicketBookingPage from "./pages/TicketBookingPage/TicketBookingPage.jsx";
import FinalBookingPage from "./pages/FinalBookingPage/FinalBookingPage.jsx";
import MyBookingsPage from "./pages/BookingsPage/MyBookingsPage.jsx";
import { StationsProvider } from './contexts/StationsContext.jsx'; // Убедись, что путь правильный

function App() {
    return (
        <BrowserRouter>
            {/* Оборачиваем все приложение (или по крайней мере Routes) в StationsProvider */}
            <StationsProvider>
                <Routes>
                    {/* Главная страница перенаправляет на /login */}
                    <Route path="/" element={<Navigate to="/login" replace />} />

                    {/* Страница входа */}
                    <Route path="/login" element={<AuthorizationPage />} />

                    {/* Защищенные маршруты */}
                    {/* FindRoutePage теперь будет иметь доступ к StationsContext */}
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
                        <Route path="/passengers-details" element={<FinalBookingPage />} />
                    </Route>

                    <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                        <Route path="/my-bookings" element={<MyBookingsPage />} />
                    </Route>

                    {/* Обработка несуществующих путей */}
                    <Route path="*" element={<div>404 Not Found</div>} /> {/* Исправлена опечатка ele ment -> element */}
                </Routes>
            </StationsProvider>
        </BrowserRouter>
    );
}

export default App;