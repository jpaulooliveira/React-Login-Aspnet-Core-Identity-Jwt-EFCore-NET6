import React from 'react';
import { Card } from "antd";
import Layout from 'components/MasterLayout';
import systemContext from 'context/SystemContext';
import RoleList from 'modules/_Manage/Security/roleList';

class Home extends React.Component {

    render() {
        return (
            <Layout>
                <Card title={`Welcome: ${this.context.user.id}`}>
                    You need to be logged in to see this!  Good Job!
                </Card>
            </Layout>

        );
    }
}

Home.contextType = systemContext;

export default Home;