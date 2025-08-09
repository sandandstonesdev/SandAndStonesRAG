import Chatbot from "./components/Chatbot";

export default function Home() {
  return (
      <div>
          <h1>Welcome to Sand and Stones AI Chatbot Website</h1>
          <main className="min-h-screen bg-gray-50 flex items-center justify-center">
              <Chatbot />
          </main>
      </div>
  );
}
