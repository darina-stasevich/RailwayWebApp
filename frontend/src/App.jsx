import {BrowserRouter, Routes, Route, Navigate, useLocation} from "react-router-dom";
import AuthorizationPage from "./pages/AuthorizationPage/AuthorizationPage.jsx";
import ProtectedRoute from "./routes/ProtectedRoute";
import FindRoutePage from "./pages/FindRoutePage/FindRoutePage.jsx";
import AdminPanel from "./pages/AdminPanel/AdminPanel.jsx";
import TicketBookingPage from "./pages/TicketBookingPage/TicketBookingPage.jsx";
import FinalBookingPage from "./pages/FinalBookingPage/FinalBookingPage.jsx";
import MyBookingsPage from "./pages/BookingsPage/MyBookingsPage.jsx";
import TicketsPage from "./pages/TicketsPage/TicketsPage.jsx";
import { StationsProvider } from './contexts/StationsContext.jsx';
import Navbar from "./utils/navigationBar/NavBar.jsx";
import ProfilePage from "./pages/ProfilePage/ProfilePage.jsx";

const AppLayout = () => {
    const location = useLocation();
    const showNavbar = location.pathname !== '/login';

    return (
        <>
            {showNavbar && <Navbar />}
            <div className="main-content">
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
                        <Route path="/passengers-details" element={<FinalBookingPage />} />
                    </Route>

                    <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                        <Route path="/my-bookings" element={<MyBookingsPage />} />
                    </Route>

                    <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                         <Route path={"/my-tickets"} element={<TicketsPage />} />
                    </Route>

                    <Route element={<ProtectedRoute allowedRoles={['Client']} />}>
                        <Route path={"/my-profile"} element={<ProfilePage />} />
                    </Route>

                    {/* Обработка несуществующих путей */}
                    <Route path="*" element={<div>404 Not Found</div>} />
                </Routes>
            </div>
        </>
    );
}


function App() {
    return (
        <BrowserRouter>
            <StationsProvider>
                <AppLayout />
            </StationsProvider>
        </BrowserRouter>
    );
}
export default App;